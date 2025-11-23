using BlazorGame.Core.Data;
using BlazorGame.Core.Services;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using SharedModels.Enums;

namespace BlazorGame.Tests;

/// <summary>
/// Tests d'intégration complets simulant une partie entière du jeu
/// </summary>
public class FullGameIntegrationTests
{
    /// Crée un contexte de base de données en mémoire pour les tests
    private GameDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GameDbContext(options);
    }

    /// Test d'intégration complet : simule une partie complète avec 5 salles
    [Fact]
    public async Task CompleteGame_WithFiveRooms_CalculatesScoreCorrectly()
    {
        // ARRANGE
        var context = CreateInMemoryContext();

        // Initialisation des services
        var rewardsService = new GameRewardsService(context);
        var roomService = new RoomTemplateService(context);
        var userService = new UserService(context);
        var sessionService = new GameSessionService(context, roomService, rewardsService, userService);
        var actionService = new GameActionService(context, rewardsService);

        // Configuration du jeu avec des valeurs fixes pour faciliter les tests
        var gameConfig = new GameRewards
        {
            Id = Guid.NewGuid(),
            NumberOfRooms = 5,
            StartingHealth = 100,
            MinCombatVictoryPoints = 100,
            MaxCombatVictoryPoints = 100,
            MinCombatDefeatPoints = -50,
            MaxCombatDefeatPoints = -50,
            MinCombatDefeatHealthLoss = -30,
            MaxCombatDefeatHealthLoss = -30,
            MinTreasurePoints = 75,
            MaxTreasurePoints = 75,
            MinPotionHealthGain = 40,
            MaxPotionHealthGain = 40,
            MinTrapPoints = -25,
            MaxTrapPoints = -25,
            MinTrapHealthLoss = -20,
            MaxTrapHealthLoss = -20,
            MinFleePoints = -15,
            MaxFleePoints = -15
        };
        context.GameRewards.Add(gameConfig);

        // Création de templates de salles variés
        var roomTemplates = new List<RoomTemplate>
        {
            new RoomTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Salle du Dragon",
                Description = "Un dragon féroce garde un trésor",
                Type = RoomType.Combat,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Bibliothèque Ancienne",
                Description = "Des livres poussiéreux et des coffres mystérieux",
                Type = RoomType.Search,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Couloir Piégé",
                Description = "Un couloir sombre avec des pièges",
                Type = RoomType.Search,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Salle des Gobelins",
                Description = "Des gobelins agressifs",
                Type = RoomType.Combat,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Sanctuaire Mystique",
                Description = "Un lieu sacré avec des potions",
                Type = RoomType.Search,
                IsActive = true
            }
        };
        context.RoomTemplates.AddRange(roomTemplates);

        // Création d'un joueur
        var playerId = Guid.NewGuid();
        var player = new User
        {
            Id = playerId,
            Username = "TestPlayer",
            Role = "player",
            IsActive = true
        };
        context.Users.Add(player);

        await context.SaveChangesAsync();

        // ACT

        // Création d'une nouvelle session de jeu
        var session = await sessionService.CreateNewSessionAsync(playerId);

        // Vérifications initiales de la session
        Assert.NotNull(session);
        Assert.Equal(playerId, session.PlayerId);
        Assert.Equal(GameStatus.InProgress, session.Status);
        Assert.Equal(100, session.CurrentHealth);
        Assert.Equal(0, session.Score);
        Assert.Equal(0, session.CurrentRoomIndex);
        Assert.Equal(5, session.TotalRooms);
        Assert.NotNull(session.GeneratedRoomsJson);

        // Simulation de 5 actions dans les 5 salles
        var actions = new List<(ActionType action, int roomNumber)>
        {
            (ActionType.Combat, 1), // Salle 1 : Combat
            (ActionType.Search, 2), // Salle 2 : Recherche
            (ActionType.Flee, 3), // Salle 3 : Fuite
            (ActionType.Combat, 4), // Salle 4 : Combat
            (ActionType.Search, 5) // Salle 5 : Recherche
        };

        var processedActions = new List<GameAction>();
        foreach (var (actionType, roomNumber) in actions)
        {
            var gameAction = await actionService.ProcessActionAsync(session.Id, actionType, roomNumber);
            processedActions.Add(gameAction);

            // Recharger la session pour avoir les dernières valeurs
            session = await sessionService.GetByIdAsync(session.Id);
            Assert.NotNull(session);
        }

        // ASSERT

        // Vérification que toutes les actions ont été enregistrées
        Assert.Equal(5, processedActions.Count);

        // Vérification que chaque action a bien été créée avec les bons attributs
        for (int i = 0; i < processedActions.Count; i++)
        {
            var action = processedActions[i];
            Assert.Equal(session.Id, action.GameSessionId);
            Assert.Equal(actions[i].action, action.Type);
            Assert.Equal(actions[i].roomNumber, action.RoomNumber);
            Assert.NotEqual(Guid.Empty, action.Id);
            Assert.True(action.Timestamp <= DateTime.UtcNow);

            // Vérification que le résultat est cohérent avec le type d'action
            switch (action.Type)
            {
                case ActionType.Combat:
                    Assert.True(action.Result == GameActionResult.Victory ||
                                action.Result == GameActionResult.Defeat);
                    break;
                case ActionType.Search:
                    Assert.True(action.Result == GameActionResult.FoundTreasure ||
                                action.Result == GameActionResult.FoundPotion ||
                                action.Result == GameActionResult.TriggeredTrap);
                    break;
                case ActionType.Flee:
                    Assert.Equal(GameActionResult.Escaped, action.Result);
                    break;
            }
        }

        // Vérification de l'état final de la session
        var finalSession = await sessionService.GetByIdAsync(session.Id);
        Assert.NotNull(finalSession);
        Assert.Equal(5, finalSession.CurrentRoomIndex); // Toutes les salles ont été visitées

        // La session devrait être terminée (Completed ou Failed selon la santé)
        Assert.True(finalSession.Status == GameStatus.Completed ||
                    finalSession.Status == GameStatus.Failed);

        // Si la session est terminée avec succès, vérifier EndTime
        if (finalSession.Status == GameStatus.Completed)
        {
            Assert.NotNull(finalSession.EndTime);
            Assert.True(finalSession.CurrentHealth > 0);
        }
        else if (finalSession.Status == GameStatus.Failed)
        {
            Assert.NotNull(finalSession.EndTime);
            Assert.Equal(0, finalSession.CurrentHealth);
        }

        // Vérification du calcul du score
        var expectedScore = processedActions.Sum(a => a.PointsChange);
        Assert.Equal(expectedScore, finalSession.Score);

        // Vérification de la santé finale
        var expectedHealth = 100 + processedActions.Sum(a => a.HealthChange);
        expectedHealth = Math.Max(0, Math.Min(200, expectedHealth)); // Clamp entre 0 et 200
        Assert.Equal(expectedHealth, finalSession.CurrentHealth);

        // Vérification que les actions sont bien liées à la session
        var sessionActions = await actionService.GetSessionActionsAsync(session.Id);
        Assert.Equal(5, sessionActions.Count);
        Assert.All(sessionActions, a => Assert.Equal(session.Id, a.GameSessionId));

        // Vérification que les actions sont triées chronologiquement
        for (int i = 0; i < sessionActions.Count - 1; i++)
        {
            Assert.True(sessionActions[i].Timestamp <= sessionActions[i + 1].Timestamp);
        }

        // Vérification de la mise à jour du joueur
        var updatedPlayer = await userService.GetUserByIdAsync(playerId);
        Assert.NotNull(updatedPlayer);
    }

    /// Test d'intégration : partie terminée par défaite (santé à 0)
    [Fact]
    public async Task CompleteGame_PlayerDies_SessionMarkedAsFailed()
    {
        // ARRANGE
        var context = CreateInMemoryContext();

        var rewardsService = new GameRewardsService(context);
        var roomService = new RoomTemplateService(context);
        var userService = new UserService(context);
        var sessionService = new GameSessionService(context, roomService, rewardsService, userService);
        var actionService = new GameActionService(context, rewardsService);

        // Configuration avec des dégâts très élevés pour garantir la mort
        var gameConfig = new GameRewards
        {
            Id = Guid.NewGuid(),
            NumberOfRooms = 5,
            StartingHealth = 100,
            MinCombatVictoryPoints = 50,
            MaxCombatVictoryPoints = 50,
            MinCombatDefeatPoints = -10,
            MaxCombatDefeatPoints = -10,
            MinCombatDefeatHealthLoss = -60, // Dégâts élevés
            MaxCombatDefeatHealthLoss = -60,
            MinTreasurePoints = 50,
            MaxTreasurePoints = 50,
            MinPotionHealthGain = 20,
            MaxPotionHealthGain = 20,
            MinTrapPoints = -10,
            MaxTrapPoints = -10,
            MinTrapHealthLoss = -60, // Dégâts élevés
            MaxTrapHealthLoss = -60,
            MinFleePoints = -5,
            MaxFleePoints = -5
        };
        context.GameRewards.Add(gameConfig);

        var roomTemplate = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Salle Mortelle",
            Description = "Une salle très dangereuse",
            Type = RoomType.Combat,
            IsActive = true
        };
        context.RoomTemplates.Add(roomTemplate);

        var playerId = Guid.NewGuid();
        var player = new User
        {
            Id = playerId,
            Username = "TestPlayer2",
            Role = "player",
            IsActive = true
        };
        context.Users.Add(player);

        await context.SaveChangesAsync();

        // ACT

        var session = await sessionService.CreateNewSessionAsync(playerId);

        // Forcer des combats jusqu'à la mort
        GameAction? lastAction = null;
        for (int i = 1; i <= 5; i++)
        {
            lastAction = await actionService.ProcessActionAsync(session.Id, ActionType.Combat, i);
            session = await sessionService.GetByIdAsync(session.Id);

            // Si le joueur est mort, arrêter
            if (session!.CurrentHealth <= 0)
            {
                break;
            }
        }

        // ASSERT

        var finalSession = await sessionService.GetByIdAsync(session!.Id);
        Assert.NotNull(finalSession);

        // Vérifier que la session est marquée comme échouée si la santé est à 0
        if (finalSession.CurrentHealth <= 0)
        {
            Assert.Equal(GameStatus.Failed, finalSession.Status);
            Assert.NotNull(finalSession.EndTime);
            Assert.Equal(0, finalSession.CurrentHealth);
        }
    }

    /// Test d'intégration : vérification du leaderboard après plusieurs parties
    [Fact]
    public async Task CompleteGame_MultipleGames_LeaderboardUpdatedCorrectly()
    {
        // ARRANGE
        var context = CreateInMemoryContext();

        var rewardsService = new GameRewardsService(context);
        var roomService = new RoomTemplateService(context);
        var userService = new UserService(context);
        var sessionService = new GameSessionService(context, roomService, rewardsService, userService);
        var actionService = new GameActionService(context, rewardsService);

        // Configuration avec des valeurs garantissant la victoire
        var gameConfig = new GameRewards
        {
            Id = Guid.NewGuid(),
            NumberOfRooms = 5,
            StartingHealth = 200, // Beaucoup de santé
            MinCombatVictoryPoints = 100,
            MaxCombatVictoryPoints = 100,
            MinCombatDefeatPoints = -10,
            MaxCombatDefeatPoints = -10,
            MinCombatDefeatHealthLoss = -5, // Peu de dégâts
            MaxCombatDefeatHealthLoss = -5,
            MinTreasurePoints = 100,
            MaxTreasurePoints = 100,
            MinPotionHealthGain = 50,
            MaxPotionHealthGain = 50,
            MinTrapPoints = -10,
            MaxTrapPoints = -10,
            MinTrapHealthLoss = -5,
            MaxTrapHealthLoss = -5,
            MinFleePoints = -5,
            MaxFleePoints = -5
        };
        context.GameRewards.Add(gameConfig);

        var roomTemplate = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Salle Test",
            Description = "Une salle de test",
            Type = RoomType.Combat,
            IsActive = true
        };
        context.RoomTemplates.Add(roomTemplate);

        // Création de 3 joueurs
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var player3Id = Guid.NewGuid();

        var players = new List<User>
        {
            new User { Id = player1Id, Username = "Player1", Role = "player", IsActive = true },
            new User { Id = player2Id, Username = "Player2", Role = "player", IsActive = true },
            new User { Id = player3Id, Username = "Player3", Role = "player", IsActive = true }
        };
        context.Users.AddRange(players);

        await context.SaveChangesAsync();

        // ACT

        // Chaque joueur joue une partie complète
        var playerSessions = new Dictionary<Guid, GameSession>();

        foreach (var player in players)
        {
            var session = await sessionService.CreateNewSessionAsync(player.Id);

            // Jouer les 5 salles avec des recherches (pour maximiser les points)
            for (int i = 1; i <= 5; i++)
            {
                await actionService.ProcessActionAsync(session.Id, ActionType.Search, i);
            }

            session = await sessionService.GetByIdAsync(session.Id);
            playerSessions[player.Id] = session!;
        }

        // ASSERT

        // Vérifier que toutes les sessions sont terminées
        foreach (var kvp in playerSessions)
        {
            var session = kvp.Value;
            Assert.True(session.Status == GameStatus.Completed || session.Status == GameStatus.Failed);
            Assert.Equal(5, session.CurrentRoomIndex);
        }

        // Vérifier que chaque session a un score calculé
        foreach (var kvp in playerSessions)
        {
            var session = kvp.Value;
            var actions = await actionService.GetSessionActionsAsync(session.Id);
            var expectedScore = actions.Sum(a => a.PointsChange);
            Assert.Equal(expectedScore, session.Score);
        }

        // Vérifier le leaderboard
        var leaderboard = await userService.GetLeaderboardAsync();
        Assert.NotNull(leaderboard);
        Assert.True(leaderboard.Count >= 3); // Au moins nos 3 joueurs

        // Vérifier que le leaderboard est trié par score décroissant
        for (int i = 0; i < leaderboard.Count - 1; i++)
        {
            Assert.True(leaderboard[i].TotalScore >= leaderboard[i + 1].TotalScore);
        }
    }

    /// Test d'intégration : abandon de partie
    [Fact]
    public async Task CompleteGame_AbandonSession_StatusUpdatedCorrectly()
    {
        // ARRANGE
        var context = CreateInMemoryContext();

        var rewardsService = new GameRewardsService(context);
        var roomService = new RoomTemplateService(context);
        var userService = new UserService(context);
        var sessionService = new GameSessionService(context, roomService, rewardsService, userService);
        var actionService = new GameActionService(context, rewardsService);

        var gameConfig = new GameRewards
        {
            Id = Guid.NewGuid(),
            NumberOfRooms = 5,
            StartingHealth = 100,
            MinCombatVictoryPoints = 50,
            MaxCombatVictoryPoints = 50,
            MinCombatDefeatPoints = -25,
            MaxCombatDefeatPoints = -25,
            MinCombatDefeatHealthLoss = -20,
            MaxCombatDefeatHealthLoss = -20,
            MinFleePoints = -10,
            MaxFleePoints = -10
        };
        context.GameRewards.Add(gameConfig);

        var roomTemplate = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Salle Test",
            Description = "Test",
            Type = RoomType.Combat,
            IsActive = true
        };
        context.RoomTemplates.Add(roomTemplate);

        var playerId = Guid.NewGuid();
        var player = new User
        {
            Id = playerId,
            Username = "TestPlayer",
            Role = "player",
            IsActive = true
        };
        context.Users.Add(player);

        await context.SaveChangesAsync();

        // ACT

        var session = await sessionService.CreateNewSessionAsync(playerId);

        // Jouer 2 salles
        await actionService.ProcessActionAsync(session.Id, ActionType.Combat, 1);
        await actionService.ProcessActionAsync(session.Id, ActionType.Flee, 2);

        // Abandonner la partie
        var abandonResult = await sessionService.AbandonSessionAsync(session.Id);

        // ASSERT

        Assert.True(abandonResult);

        var abandonedSession = await sessionService.GetByIdAsync(session.Id);
        Assert.NotNull(abandonedSession);
        Assert.Equal(GameStatus.Abandoned, abandonedSession.Status);
        Assert.NotNull(abandonedSession.EndTime);
        Assert.Equal(2, abandonedSession.CurrentRoomIndex); // 2 salles jouées
        Assert.True(abandonedSession.CurrentRoomIndex < abandonedSession.TotalRooms);

        // Vérifier que la session ne peut plus continuer
        Assert.False(sessionService.CanContinue(abandonedSession));
    }

    /// Test d'intégration : vérification de la génération aléatoire des salles
    [Fact]
    public async Task CompleteGame_GeneratesRooms_FromAvailableTemplates()
    {
        // ARRANGE
        var context = CreateInMemoryContext();

        var rewardsService = new GameRewardsService(context);
        var roomService = new RoomTemplateService(context);
        var userService = new UserService(context);
        var sessionService = new GameSessionService(context, roomService, rewardsService, userService);

        var gameConfig = new GameRewards
        {
            Id = Guid.NewGuid(),
            NumberOfRooms = 5,
            StartingHealth = 100
        };
        context.GameRewards.Add(gameConfig);

        // Créer plusieurs templates
        var templates = new List<RoomTemplate>
        {
            new RoomTemplate
                { Id = Guid.NewGuid(), Name = "Room1", Description = "D1", Type = RoomType.Combat, IsActive = true },
            new RoomTemplate
                { Id = Guid.NewGuid(), Name = "Room2", Description = "D2", Type = RoomType.Search, IsActive = true },
            new RoomTemplate
                { Id = Guid.NewGuid(), Name = "Room3", Description = "D3", Type = RoomType.Search, IsActive = true }
        };
        context.RoomTemplates.AddRange(templates);

        var playerId = Guid.NewGuid();
        var player = new User { Id = playerId, Username = "Test", Role = "player", IsActive = true };
        context.Users.Add(player);

        await context.SaveChangesAsync();

        // ACT

        var session = await sessionService.CreateNewSessionAsync(playerId);

        // ASSERT

        Assert.NotNull(session.GeneratedRoomsJson);
        Assert.NotEqual("[]", session.GeneratedRoomsJson);

        // Désérialiser et vérifier que les IDs correspondent à des templates existants
        var generatedRoomIds = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(session.GeneratedRoomsJson);
        Assert.NotNull(generatedRoomIds);
        Assert.Equal(5, generatedRoomIds.Count);

        var templateIds = templates.Select(t => t.Id).ToList();
        foreach (var roomId in generatedRoomIds)
        {
            Assert.Contains(roomId, templateIds);
        }
    }
}

