using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using SharedModels.Enums;

namespace BlazorGame.Core.Data;

/// <summary>
/// Classe statique pour les données de seed de la base de données
/// </summary>
public static class GameDbContextSeeder
{
    public static void SeedData(ModelBuilder modelBuilder)
    {
        // Configuration des récompenses et pénalités
        modelBuilder.Entity<GameRewards>().HasData(
            new GameRewards
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                MinCombatVictoryPoints = 80,
                MaxCombatVictoryPoints = 120,
                MinCombatDefeatPoints = -60,
                MaxCombatDefeatPoints = -40,
                MinCombatDefeatHealthLoss = -40,
                MaxCombatDefeatHealthLoss = -20,
                MinTreasurePoints = 60,
                MaxTreasurePoints = 90,
                MinPotionHealthGain = 30,
                MaxPotionHealthGain = 50,
                MinTrapPoints = -35,
                MaxTrapPoints = -15,
                MinTrapHealthLoss = -30,
                MaxTrapHealthLoss = -10,
                MinFleePoints = -20,
                MaxFleePoints = -10,
                NumberOfRooms = 5,
                StartingHealth = 100
            }
        );

        // Création des utilisateurs de test correspondant aux comptes Keycloak
        // Ces GUIDs doivent correspondre aux IDs Keycloak des utilisateurs
        var user1Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var user2Id = Guid.Parse("22222222-2222-2222-2222-222222222223");

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = user1Id,
                Username = "user1",
                Role = "joueur",
                IsActive = true,
                LastConnectionDate = DateTime.UtcNow.AddDays(-5)
            },
            new User
            {
                Id = user2Id,
                Username = "user2",
                Role = "joueur",
                IsActive = true,
                LastConnectionDate = DateTime.UtcNow.AddDays(-4)
            }
        );

        // Salles de Combat
        modelBuilder.Entity<RoomTemplate>().HasData(
            new RoomTemplate
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Gobelin féroce",
                Description =
                    "Un gobelin aux yeux rouges bondit devant vous, brandissant une épée rouillée. Il grogne de manière menaçante et bloque votre passage. Allez-vous l'affronter ?",
                Type = RoomType.Combat,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Squelette gardien",
                Description =
                    "Les ossements d'un ancien guerrier s'animent devant vous. Ses yeux vides brillent d'une lueur bleue tandis qu'il brandit une épée spectrale. Le combat semble inévitable.",
                Type = RoomType.Combat,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Name = "Araignée géante",
                Description =
                    "Une araignée monstrueuse descend du plafond, ses crochets venimeux claquant dans l'obscurité. Ses huit yeux vous fixent avec une faim dévorante.",
                Type = RoomType.Combat,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Name = "Orc sanguinaire",
                Description =
                    "Un orc massif bloque le passage, sa hache de guerre couverte de sang séché. Il rugit en vous voyant et se prépare à charger.",
                Type = RoomType.Combat,
                IsActive = true
            }
        );

        // Salles de Fouille
        modelBuilder.Entity<RoomTemplate>().HasData(
            new RoomTemplate
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Coffre mystérieux",
                Description =
                    "Un coffre en bois orné de runes anciennes trône au centre de la salle. Il semble intact mais pourrait contenir un trésor... ou un piège mortel.",
                Type = RoomType.Search,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Name = "Bibliothèque abandonnée",
                Description =
                    "Des étagères poussiéreuses remplies de grimoires anciens tapissent les murs. Certains livres brillent faiblement. Que pourriez-vous découvrir ici ?",
                Type = RoomType.Search,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Name = "Autel oublié",
                Description =
                    "Un autel de pierre se dresse au centre de la pièce, entouré de chandelles éteintes. Des offrandes anciennes sont éparpillées autour. Osez-vous les toucher ?",
                Type = RoomType.Search,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Salle du trésor",
                Description =
                    "Des pièces d'or et des bijoux scintillent dans la pénombre. Mais attention, les trésors les plus brillants cachent souvent les pièges les plus mortels...",
                Type = RoomType.Search,
                IsActive = true
            }
        );

        // Parties de démonstration pour user1
        // Partie 1 : Partie complétée avec succès
        var user1Session1Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01");
        var user1Session1StartTime = DateTime.UtcNow.AddDays(-5);

        modelBuilder.Entity<GameSession>().HasData(
            new GameSession
            {
                Id = user1Session1Id,
                PlayerId = user1Id,
                StartTime = user1Session1StartTime,
                EndTime = user1Session1StartTime.AddMinutes(25),
                LastSaveTime = user1Session1StartTime.AddMinutes(25),
                Score = 420,
                CurrentHealth = 75,
                CurrentRoomIndex = 5,
                TotalRooms = 5,
                Status = GameStatus.Completed,
                GeneratedRoomsJson =
                    "[\"33333333-3333-3333-3333-333333333333\",\"44444444-4444-4444-4444-444444444444\",\"55555555-5555-5555-5555-555555555555\",\"88888888-8888-8888-8888-888888888888\",\"66666666-6666-6666-6666-666666666666\"]"
            }
        );

        // Actions pour la partie complétée de user1
        modelBuilder.Entity<GameAction>().HasData(
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc01"),
                GameSessionId = user1Session1Id,
                Timestamp = user1Session1StartTime.AddMinutes(3),
                Type = ActionType.Combat,
                Result = GameActionResult.Victory,
                PointsChange = 100,
                HealthChange = 0,
                RoomNumber = 1
            },
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc02"),
                GameSessionId = user1Session1Id,
                Timestamp = user1Session1StartTime.AddMinutes(8),
                Type = ActionType.Search,
                Result = GameActionResult.FoundTreasure,
                PointsChange = 75,
                HealthChange = 0,
                RoomNumber = 2
            },
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc03"),
                GameSessionId = user1Session1Id,
                Timestamp = user1Session1StartTime.AddMinutes(14),
                Type = ActionType.Combat,
                Result = GameActionResult.Defeat,
                PointsChange = -50,
                HealthChange = -25,
                RoomNumber = 3
            },
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc04"),
                GameSessionId = user1Session1Id,
                Timestamp = user1Session1StartTime.AddMinutes(19),
                Type = ActionType.Search,
                Result = GameActionResult.FoundPotion,
                PointsChange = 0,
                HealthChange = 40,
                RoomNumber = 4
            },
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc05"),
                GameSessionId = user1Session1Id,
                Timestamp = user1Session1StartTime.AddMinutes(25),
                Type = ActionType.Combat,
                Result = GameActionResult.Victory,
                PointsChange = 95,
                HealthChange = 0,
                RoomNumber = 5
            }
        );

        // Partie 2 : Partie abandonnée par user1
        var user1Session2Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02");
        var user1Session2StartTime = DateTime.UtcNow.AddDays(-3);

        modelBuilder.Entity<GameSession>().HasData(
            new GameSession
            {
                Id = user1Session2Id,
                PlayerId = user1Id,
                StartTime = user1Session2StartTime,
                EndTime = user1Session2StartTime.AddMinutes(10),
                LastSaveTime = user1Session2StartTime.AddMinutes(10),
                Score = 45,
                CurrentHealth = 60,
                CurrentRoomIndex = 2,
                TotalRooms = 5,
                Status = GameStatus.Abandoned,
                GeneratedRoomsJson =
                    "[\"77777777-7777-7777-7777-777777777777\",\"99999999-9999-9999-9999-999999999999\",\"55555555-5555-5555-5555-555555555555\",\"44444444-4444-4444-4444-444444444444\",\"66666666-6666-6666-6666-666666666666\"]"
            }
        );

        // Actions pour la partie abandonnée de user1
        modelBuilder.Entity<GameAction>().HasData(
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc06"),
                GameSessionId = user1Session2Id,
                Timestamp = user1Session2StartTime.AddMinutes(5),
                Type = ActionType.Combat,
                Result = GameActionResult.Defeat,
                PointsChange = -45,
                HealthChange = -30,
                RoomNumber = 1
            },
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc07"),
                GameSessionId = user1Session2Id,
                Timestamp = user1Session2StartTime.AddMinutes(10),
                Type = ActionType.Search,
                Result = GameActionResult.FoundTreasure,
                PointsChange = 70,
                HealthChange = 0,
                RoomNumber = 2
            }
        );

        // Parties de démonstration pour user2
        // Partie 1 : Partie échouée (mort)
        var user2Session1Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03");
        var user2Session1StartTime = DateTime.UtcNow.AddDays(-4);

        modelBuilder.Entity<GameSession>().HasData(
            new GameSession
            {
                Id = user2Session1Id,
                PlayerId = user2Id,
                StartTime = user2Session1StartTime,
                EndTime = user2Session1StartTime.AddMinutes(18),
                LastSaveTime = user2Session1StartTime.AddMinutes(18),
                Score = 150,
                CurrentHealth = 0,
                CurrentRoomIndex = 4,
                TotalRooms = 5,
                Status = GameStatus.Failed,
                GeneratedRoomsJson =
                    "[\"66666666-6666-6666-6666-666666666666\",\"88888888-8888-8888-8888-888888888888\",\"77777777-7777-7777-7777-777777777777\",\"33333333-3333-3333-3333-333333333333\",\"55555555-5555-5555-5555-555555555555\"]"
            }
        );

        // Actions pour la partie échouée de user2
        modelBuilder.Entity<GameAction>().HasData(
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc08"),
                GameSessionId = user2Session1Id,
                Timestamp = user2Session1StartTime.AddMinutes(4),
                Type = ActionType.Combat,
                Result = GameActionResult.Victory,
                PointsChange = 110,
                HealthChange = 0,
                RoomNumber = 1
            },
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc09"),
                GameSessionId = user2Session1Id,
                Timestamp = user2Session1StartTime.AddMinutes(9),
                Type = ActionType.Search,
                Result = GameActionResult.TriggeredTrap,
                PointsChange = -25,
                HealthChange = -20,
                RoomNumber = 2
            },
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc10"),
                GameSessionId = user2Session1Id,
                Timestamp = user2Session1StartTime.AddMinutes(13),
                Type = ActionType.Combat,
                Result = GameActionResult.Defeat,
                PointsChange = -55,
                HealthChange = -35,
                RoomNumber = 3
            },
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc11"),
                GameSessionId = user2Session1Id,
                Timestamp = user2Session1StartTime.AddMinutes(18),
                Type = ActionType.Combat,
                Result = GameActionResult.Defeat,
                PointsChange = -60,
                HealthChange = -45,
                RoomNumber = 4
            }
        );

        // Partie 2 : Partie complétée avec succès par user2
        var user2Session2Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb04");
        var user2Session2StartTime = DateTime.UtcNow.AddDays(-2);

        modelBuilder.Entity<GameSession>().HasData(
            new GameSession
            {
                Id = user2Session2Id,
                PlayerId = user2Id,
                StartTime = user2Session2StartTime,
                EndTime = user2Session2StartTime.AddMinutes(30),
                LastSaveTime = user2Session2StartTime.AddMinutes(30),
                Score = 510,
                CurrentHealth = 90,
                CurrentRoomIndex = 5,
                TotalRooms = 5,
                Status = GameStatus.Completed,
                GeneratedRoomsJson =
                    "[\"44444444-4444-4444-4444-444444444444\",\"33333333-3333-3333-3333-333333333333\",\"88888888-8888-8888-8888-888888888888\",\"55555555-5555-5555-5555-555555555555\",\"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa\"]"
            }
        );

        // Actions pour la partie complétée de user2
        modelBuilder.Entity<GameAction>().HasData(
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc12"),
                GameSessionId = user2Session2Id,
                Timestamp = user2Session2StartTime.AddMinutes(5),
                Type = ActionType.Search,
                Result = GameActionResult.FoundPotion,
                PointsChange = 0,
                HealthChange = 45,
                RoomNumber = 1
            },
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc13"),
                GameSessionId = user2Session2Id,
                Timestamp = user2Session2StartTime.AddMinutes(11),
                Type = ActionType.Combat,
                Result = GameActionResult.Victory,
                PointsChange = 115,
                HealthChange = 0,
                RoomNumber = 2
            },
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc14"),
                GameSessionId = user2Session2Id,
                Timestamp = user2Session2StartTime.AddMinutes(17),
                Type = ActionType.Search,
                Result = GameActionResult.FoundTreasure,
                PointsChange = 85,
                HealthChange = 0,
                RoomNumber = 3
            },
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc15"),
                GameSessionId = user2Session2Id,
                Timestamp = user2Session2StartTime.AddMinutes(23),
                Type = ActionType.Combat,
                Result = GameActionResult.Victory,
                PointsChange = 105,
                HealthChange = 0,
                RoomNumber = 4
            },
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc16"),
                GameSessionId = user2Session2Id,
                Timestamp = user2Session2StartTime.AddMinutes(30),
                Type = ActionType.Search,
                Result = GameActionResult.FoundTreasure,
                PointsChange = 85,
                HealthChange = 0,
                RoomNumber = 5
            }
        );

        // Partie 3 : Partie abandonnée par user2
        var user2Session3Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb05");
        var user2Session3StartTime = DateTime.UtcNow.AddDays(-1);

        modelBuilder.Entity<GameSession>().HasData(
            new GameSession
            {
                Id = user2Session3Id,
                PlayerId = user2Id,
                StartTime = user2Session3StartTime,
                EndTime = user2Session3StartTime.AddMinutes(7),
                LastSaveTime = user2Session3StartTime.AddMinutes(7),
                Score = -15,
                CurrentHealth = 85,
                CurrentRoomIndex = 1,
                TotalRooms = 5,
                Status = GameStatus.Abandoned,
                GeneratedRoomsJson =
                    "[\"77777777-7777-7777-7777-777777777777\",\"44444444-4444-4444-4444-444444444444\",\"66666666-6666-6666-6666-666666666666\",\"99999999-9999-9999-9999-999999999999\",\"33333333-3333-3333-3333-333333333333\"]"
            }
        );

        // Actions pour la partie abandonnée de user2
        modelBuilder.Entity<GameAction>().HasData(
            new GameAction
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccc17"),
                GameSessionId = user2Session3Id,
                Timestamp = user2Session3StartTime.AddMinutes(7),
                Type = ActionType.Flee,
                Result = GameActionResult.Escaped,
                PointsChange = -15,
                HealthChange = 0,
                RoomNumber = 1
            }
        );
    }
}
