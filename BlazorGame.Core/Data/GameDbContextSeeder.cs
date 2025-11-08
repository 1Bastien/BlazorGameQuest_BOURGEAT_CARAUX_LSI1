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

        // Salles de Combat
        modelBuilder.Entity<RoomTemplate>().HasData(
            new RoomTemplate
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Gobelin féroce",
                Description = "Un gobelin aux yeux rouges bondit devant vous, brandissant une épée rouillée. Il grogne de manière menaçante et bloque votre passage. Allez-vous l'affronter ?",
                Type = RoomType.Combat,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Squelette gardien",
                Description = "Les ossements d'un ancien guerrier s'animent devant vous. Ses yeux vides brillent d'une lueur bleue tandis qu'il brandit une épée spectrale. Le combat semble inévitable.",
                Type = RoomType.Combat,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Name = "Araignée géante",
                Description = "Une araignée monstrueuse descend du plafond, ses crochets venimeux claquant dans l'obscurité. Ses huit yeux vous fixent avec une faim dévorante.",
                Type = RoomType.Combat,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Name = "Orc sanguinaire",
                Description = "Un orc massif bloque le passage, sa hache de guerre couverte de sang séché. Il rugit en vous voyant et se prépare à charger.",
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
                Description = "Un coffre en bois orné de runes anciennes trône au centre de la salle. Il semble intact mais pourrait contenir un trésor... ou un piège mortel.",
                Type = RoomType.Search,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Name = "Bibliothèque abandonnée",
                Description = "Des étagères poussiéreuses remplies de grimoires anciens tapissent les murs. Certains livres brillent faiblement. Que pourriez-vous découvrir ici ?",
                Type = RoomType.Search,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Name = "Autel oublié",
                Description = "Un autel de pierre se dresse au centre de la pièce, entouré de chandelles éteintes. Des offrandes anciennes sont éparpillées autour. Osez-vous les toucher ?",
                Type = RoomType.Search,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name = "Salle du trésor",
                Description = "Des pièces d'or et des bijoux scintillent dans la pénombre. Mais attention, les trésors les plus brillants cachent souvent les pièges les plus mortels...",
                Type = RoomType.Search,
                IsActive = true
            }
        );
    }
}
