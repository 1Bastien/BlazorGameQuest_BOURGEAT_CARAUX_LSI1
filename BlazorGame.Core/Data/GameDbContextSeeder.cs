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
                MaxRooms = 5,
                StartingHealth = 100
            }
        );

        // Joueur de test
        var playerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = playerId,
                Username = "testplayer",
                Email = "player@blazorgame.com",
                PasswordHash = "TEST",
                Role = UserRole.Player,
                IsActive = true
            }
        );

        // Salle de Combat
        modelBuilder.Entity<RoomTemplate>().HasData(
            new RoomTemplate
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Gobelin féroce",
                Description = "Un gobelin aux yeux rouges bondit devant vous, brandissant une épée rouillée. Il grogne de manière menaçante et bloque votre passage. Allez-vous l'affronter ?",
                Type = RoomType.Combat,
                IsActive = true
            }
        );

        // Salle de Fouille
        modelBuilder.Entity<RoomTemplate>().HasData(
            new RoomTemplate
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Coffre mystérieux",
                Description = "Un coffre en bois orné de runes anciennes trône au centre de la salle. Il semble intact mais pourrait contenir un trésor... ou un piège mortel.",
                Type = RoomType.Search,
                IsActive = true
            }
        );
    }
}
