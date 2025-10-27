using BlazorGame.Core.Data;
using BlazorGame.Core.Services;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using SharedModels.Enums;

namespace BlazorGame.Tests;

/// <summary>
/// Tests unitaires pour le service GameActionService
/// </summary>
public class GameActionServiceTests
{
    /// Crée un contexte de base de données en mémoire pour les tests
    private GameDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GameDbContext(options);
    }

    /// Prépare les données de test dans le contexte
    private async Task<(GameDbContext context, GameSession session, GameRewards config)> SetupTestDataAsync()
    {
        var context = CreateInMemoryContext();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "TestUser",
            Email = "test@test.com",
            PasswordHash = "motdepasse"
        };

        var config = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 100,
            MaxRooms = 10,
            MinCombatVictoryPoints = 10,
            MaxCombatVictoryPoints = 20,
            MinCombatDefeatPoints = -5,
            MaxCombatDefeatPoints = 0,
            MinCombatDefeatHealthLoss = -20,
            MaxCombatDefeatHealthLoss = -10,
            MinTreasurePoints = 15,
            MaxTreasurePoints = 30,
            MinPotionHealthGain = 10,
            MaxPotionHealthGain = 20,
            MinTrapPoints = -10,
            MaxTrapPoints = 0,
            MinTrapHealthLoss = -15,
            MaxTrapHealthLoss = -5,
            MinFleePoints = -5,
            MaxFleePoints = 0
        };

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            Score = 50,
            CurrentHealth = 100,
            CurrentRoomIndex = 0,
            TotalRooms = 5,
            Status = GameStatus.InProgress,
            GeneratedRoomsJson = "[]"
        };

        context.Users.Add(user);
        context.GameRewards.Add(config);
        context.GameSessions.Add(session);
        await context.SaveChangesAsync();

        return (context, session, config);
    }

    /// Test ProcessActionAsync succès
    [Fact]
    public async Task ProcessActionAsync_ProcessesAction_WhenDataValid()
    {
        // Arrange
        var (context, session, config) = await SetupTestDataAsync();
        var rewardsService = new GameRewardsService(context);
        var service = new GameActionService(context, rewardsService);

        // Act
        var result = await service.ProcessActionAsync(session.Id, ActionType.Flee, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(session.Id, result.GameSessionId);
        Assert.Equal(ActionType.Flee, result.Type);
        Assert.Equal(GameActionResult.Escaped, result.Result);
        Assert.Equal(1, result.RoomNumber);
        Assert.InRange(result.PointsChange, config.MinFleePoints, config.MaxFleePoints);

        var updatedSession = await context.GameSessions.FindAsync(session.Id);
        Assert.Equal(1, updatedSession!.CurrentRoomIndex);
    }

    /// Test ProcessActionAsync lève une exception quand la session n'existe pas
    [Fact]
    public async Task ProcessActionAsync_ThrowsException_WhenSessionNotFound()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var config = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 100,
            MaxRooms = 10
        };
        context.GameRewards.Add(config);
        await context.SaveChangesAsync();

        var rewardsService = new GameRewardsService(context);
        var service = new GameActionService(context, rewardsService);
        var nonExistentSessionId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ProcessActionAsync(nonExistentSessionId, ActionType.Combat, 1)
        );
    }

    /// Test ProcessActionAsync lève une exception quand la config n'existe pas
    [Fact]
    public async Task ProcessActionAsync_ThrowsException_WhenConfigNotFound()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var user = new User
            { Id = Guid.NewGuid(), Username = "TestUser", Email = "test@test.com", PasswordHash = "hash" };
        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        context.Users.Add(user);
        context.GameSessions.Add(session);
        await context.SaveChangesAsync();

        var rewardsService = new GameRewardsService(context);
        var service = new GameActionService(context, rewardsService);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ProcessActionAsync(session.Id, ActionType.Combat, 1)
        );
    }

    /// Test ProcessActionAsync met le statut à Failed quand la santé atteint 0
    [Fact]
    public async Task ProcessActionAsync_SetsStatusToFailed_WhenHealthReachesZero()
    {
        // Arrange
        var (context, session, config) = await SetupTestDataAsync();
        session.CurrentHealth = 1; // Santé très basse
        await context.SaveChangesAsync();

        // Forcer des valeurs de config qui garantissent une perte de santé importante
        config.MinCombatDefeatHealthLoss = -50;
        config.MaxCombatDefeatHealthLoss = -50;
        await context.SaveChangesAsync();

        var rewardsService = new GameRewardsService(context);
        var service = new GameActionService(context, rewardsService);

        // Act - On fait plusieurs tentatives pour avoir un combat perdu
        for (int i = 0; i < 20; i++)
        {
            session = await context.GameSessions.FindAsync(session.Id);
            if (session!.Status == GameStatus.Failed) break;

            session.CurrentRoomIndex = 0; // Reset pour permettre une autre action
            session.Status = GameStatus.InProgress;
            await context.SaveChangesAsync();

            await service.ProcessActionAsync(session.Id, ActionType.Combat, i);
        }

        // Assert
        var finalSession = await context.GameSessions.FindAsync(session.Id);
        Assert.Equal(GameStatus.Failed, finalSession!.Status);
        Assert.NotNull(finalSession.EndTime);
    }

    /// Test ProcessActionAsync met le statut à Completed quand toutes les salles sont terminées
    [Fact]
    public async Task ProcessActionAsync_SetsStatusToCompleted_WhenAllRoomsCompleted()
    {
        // Arrange
        var (context, session, _) = await SetupTestDataAsync();
        session.CurrentRoomIndex = session.TotalRooms - 1; // Dernière salle
        await context.SaveChangesAsync();

        var rewardsService = new GameRewardsService(context);
        var service = new GameActionService(context, rewardsService);

        // Act
        await service.ProcessActionAsync(session.Id, ActionType.Flee, session.TotalRooms);

        // Assert
        var updatedSession = await context.GameSessions.FindAsync(session.Id);
        Assert.Equal(GameStatus.Completed, updatedSession!.Status);
        Assert.NotNull(updatedSession.EndTime);
        Assert.Equal(session.TotalRooms, updatedSession.CurrentRoomIndex);
    }

    /// Test ProcessActionAsync gère correctement l'action Combat
    [Fact]
    public async Task ProcessActionAsync_HandlesCombatAction_Correctly()
    {
        // Arrange
        var (context, session, config) = await SetupTestDataAsync();
        var rewardsService = new GameRewardsService(context);
        var service = new GameActionService(context, rewardsService);

        // Act
        var result = await service.ProcessActionAsync(session.Id, ActionType.Combat, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ActionType.Combat, result.Type);
        Assert.True(result.Result == GameActionResult.Victory || result.Result == GameActionResult.Defeat);

        if (result.Result == GameActionResult.Victory)
        {
            Assert.InRange(result.PointsChange, config.MinCombatVictoryPoints, config.MaxCombatVictoryPoints);
            Assert.Equal(0, result.HealthChange);
        }
        else
        {
            Assert.InRange(result.PointsChange, config.MinCombatDefeatPoints, config.MaxCombatDefeatPoints);
            Assert.InRange(result.HealthChange, config.MinCombatDefeatHealthLoss, config.MaxCombatDefeatHealthLoss);
        }
    }

    /// Test ProcessActionAsync gère correctement l'action Search
    [Fact]
    public async Task ProcessActionAsync_HandlesSearchAction_Correctly()
    {
        // Arrange
        var (context, session, config) = await SetupTestDataAsync();
        var rewardsService = new GameRewardsService(context);
        var service = new GameActionService(context, rewardsService);

        // Act
        var result = await service.ProcessActionAsync(session.Id, ActionType.Search, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ActionType.Search, result.Type);
        Assert.True(
            result.Result == GameActionResult.FoundTreasure ||
            result.Result == GameActionResult.FoundPotion ||
            result.Result == GameActionResult.TriggeredTrap
        );

        // Vérifier les valeurs selon le résultat
        if (result.Result == GameActionResult.FoundTreasure)
        {
            Assert.InRange(result.PointsChange, config.MinTreasurePoints, config.MaxTreasurePoints);
            Assert.Equal(0, result.HealthChange);
        }
        else if (result.Result == GameActionResult.FoundPotion)
        {
            Assert.Equal(0, result.PointsChange);
            Assert.InRange(result.HealthChange, config.MinPotionHealthGain, config.MaxPotionHealthGain);
        }
        else // TriggeredTrap
        {
            Assert.InRange(result.PointsChange, config.MinTrapPoints, config.MaxTrapPoints);
            Assert.InRange(result.HealthChange, config.MinTrapHealthLoss, config.MaxTrapHealthLoss);
        }
    }
}

