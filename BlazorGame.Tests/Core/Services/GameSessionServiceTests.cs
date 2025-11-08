using BlazorGame.Core.Data;
using BlazorGame.Core.Services;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using SharedModels.Enums;

namespace BlazorGame.Tests;

/// <summary>
/// Tests unitaires pour le service GameSessionService
/// </summary>
public class GameSessionServiceTests
{
    /// Crée un contexte de base de données en mémoire pour les tests
    private GameDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GameDbContext(options);
    }

    /// Test: GetPlayerSessionsAsync retourne toutes les sessions d'un joueur triées par date décroissante
    [Fact]
    public async Task GetPlayerSessionsAsync_ReturnsPlayerSessions_OrderedByStartTimeDesc()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var roomService = new RoomTemplateService(context);
        var rewardsService = new GameRewardsService(context);
        var service = new GameSessionService(context, roomService, rewardsService);

        var user1 = new User
            { Id = Guid.NewGuid(), Username = "TestUser1", Email = "test1@test.com", PasswordHash = "hash" };
        var user2 = new User
            { Id = Guid.NewGuid(), Username = "TestUser2", Email = "test2@test.com", PasswordHash = "hash" };
        context.Users.AddRange(user1, user2);

        var session1 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user1.Id,
            StartTime = DateTime.UtcNow.AddHours(-2),
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var session2 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user1.Id,
            StartTime = DateTime.UtcNow,
            TotalRooms = 3,
            GeneratedRoomsJson = "[]"
        };

        var session3 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user2.Id,
            StartTime = DateTime.UtcNow.AddHours(-1),
            TotalRooms = 4,
            GeneratedRoomsJson = "[]"
        };

        context.GameSessions.AddRange(session1, session2, session3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPlayerSessionsAsync(user1.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(session2.Id, result[0].Id); // Plus récent en premier
        Assert.Equal(session1.Id, result[1].Id);
        Assert.All(result, s => Assert.Equal(user1.Id, s.PlayerId)); // Toutes les sessions appartiennent à user1
    }

    /// Test: GetPlayerSessionsAsync retourne une liste vide quand le joueur n'a pas de sessions
    [Fact]
    public async Task GetPlayerSessionsAsync_ReturnsEmptyList_WhenNoSessions()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var roomService = new RoomTemplateService(context);
        var rewardsService = new GameRewardsService(context);
        var service = new GameSessionService(context, roomService, rewardsService);

        var userId = Guid.NewGuid();

        // Act
        var result = await service.GetPlayerSessionsAsync(userId);

        // Assert
        Assert.Empty(result);
    }

    /// Test: GetByIdAsync retourne la session avec ses relations
    [Fact]
    public async Task GetByIdAsync_ReturnsSession_WhenExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var roomService = new RoomTemplateService(context);
        var rewardsService = new GameRewardsService(context);
        var service = new GameSessionService(context, roomService, rewardsService);

        var user = new User
            { Id = Guid.NewGuid(), Username = "TestUser", Email = "test@test.com", PasswordHash = "hash" };
        context.Users.Add(user);

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };
        context.GameSessions.Add(session);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetByIdAsync(session.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(session.Id, result.Id);
        Assert.NotNull(result.Player);
        Assert.NotNull(result.Actions);
    }

    /// Test: GetByIdAsync retourne null quand la session n'existe pas
    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var roomService = new RoomTemplateService(context);
        var rewardsService = new GameRewardsService(context);
        var service = new GameSessionService(context, roomService, rewardsService);

        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await service.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    /// Test: CreateNewSessionAsync crée une nouvelle session avec succès
    [Fact]
    public async Task CreateNewSessionAsync_CreatesSession_WhenConfigExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var roomService = new RoomTemplateService(context);
        var rewardsService = new GameRewardsService(context);
        var service = new GameSessionService(context, roomService, rewardsService);

        var user = new User
            { Id = Guid.NewGuid(), Username = "TestUser", Email = "test@test.com", PasswordHash = "hash" };
        context.Users.Add(user);

        var config = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 100,
            MinCombatVictoryPoints = 10,
            MaxCombatVictoryPoints = 20
        };
        context.GameRewards.Add(config);

        var template = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Test Room",
            Description = "Test",
            Type = RoomType.Combat,
            IsActive = true
        };
        context.RoomTemplates.Add(template);
        await context.SaveChangesAsync();

        // Act
        var result = await service.CreateNewSessionAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.PlayerId);
        Assert.Equal(GameStatus.InProgress, result.Status);
        Assert.Equal(config.StartingHealth, result.CurrentHealth);
        Assert.Equal(0, result.Score);
        Assert.Equal(0, result.CurrentRoomIndex);
        Assert.True(result.TotalRooms > 0);
    }

    /// Test: CreateNewSessionAsync lève une exception quand aucun template n'existe
    [Fact]
    public async Task CreateNewSessionAsync_ThrowsException_WhenNoTemplates()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var roomService = new RoomTemplateService(context);
        var rewardsService = new GameRewardsService(context);
        var service = new GameSessionService(context, roomService, rewardsService);

        var config = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 100,
        };
        context.GameRewards.Add(config);
        await context.SaveChangesAsync();

        var userId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateNewSessionAsync(userId)
        );
    }

    /// Test: UpdateSessionAsync met à jour une session existante
    [Fact]
    public async Task UpdateSessionAsync_UpdatesSession_WhenExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var roomService = new RoomTemplateService(context);
        var rewardsService = new GameRewardsService(context);
        var service = new GameSessionService(context, roomService, rewardsService);

        var user = new User
            { Id = Guid.NewGuid(), Username = "TestUser", Email = "test@test.com", PasswordHash = "hash" };
        context.Users.Add(user);

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            Score = 0,
            CurrentHealth = 100,
            CurrentRoomIndex = 0,
            TotalRooms = 5,
            Status = GameStatus.InProgress,
            GeneratedRoomsJson = "[]"
        };
        context.GameSessions.Add(session);
        await context.SaveChangesAsync();

        var updatedSession = new GameSession
        {
            Id = session.Id,
            Score = 50,
            CurrentHealth = 80,
            CurrentRoomIndex = 2,
            Status = GameStatus.InProgress
        };

        // Act
        var result = await service.UpdateSessionAsync(session.Id, updatedSession);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(50, result.Score);
        Assert.Equal(80, result.CurrentHealth);
        Assert.Equal(2, result.CurrentRoomIndex);
    }

    /// Test: UpdateSessionAsync retourne null quand la session n'existe pas
    [Fact]
    public async Task UpdateSessionAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var roomService = new RoomTemplateService(context);
        var rewardsService = new GameRewardsService(context);
        var service = new GameSessionService(context, roomService, rewardsService);

        var nonExistentId = Guid.NewGuid();
        var session = new GameSession
        {
            Id = nonExistentId,
            Score = 50,
            CurrentHealth = 80,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        // Act
        var result = await service.UpdateSessionAsync(nonExistentId, session);

        // Assert
        Assert.Null(result);
    }

    /// Test: AbandonSessionAsync marque une session comme abandonnée
    [Fact]
    public async Task AbandonSessionAsync_AbandonsSession_WhenExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var roomService = new RoomTemplateService(context);
        var rewardsService = new GameRewardsService(context);
        var service = new GameSessionService(context, roomService, rewardsService);

        var user = new User
            { Id = Guid.NewGuid(), Username = "TestUser", Email = "test@test.com", PasswordHash = "hash" };
        context.Users.Add(user);

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            Status = GameStatus.InProgress,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };
        context.GameSessions.Add(session);
        await context.SaveChangesAsync();

        // Act
        var result = await service.AbandonSessionAsync(session.Id);

        // Assert
        Assert.True(result);
        var abandonedSession = await context.GameSessions.FindAsync(session.Id);
        Assert.Equal(GameStatus.Abandoned, abandonedSession!.Status);
        Assert.NotNull(abandonedSession.EndTime);
    }

    /// Test: AbandonSessionAsync retourne false quand la session n'existe pas
    [Fact]
    public async Task AbandonSessionAsync_ReturnsFalse_WhenNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var roomService = new RoomTemplateService(context);
        var rewardsService = new GameRewardsService(context);
        var service = new GameSessionService(context, roomService, rewardsService);

        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await service.AbandonSessionAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    /// Test: CanContinue retourne true quand la session peut continuer
    [Fact]
    public void CanContinue_ReturnsTrue_WhenSessionCanContinue()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var roomService = new RoomTemplateService(context);
        var rewardsService = new GameRewardsService(context);
        var service = new GameSessionService(context, roomService, rewardsService);

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            Status = GameStatus.InProgress,
            CurrentHealth = 50,
            CurrentRoomIndex = 2,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        // Act
        var result = service.CanContinue(session);

        // Assert
        Assert.True(result);
    }

    /// Test: CanContinue retourne false quand la session ne peut pas continuer
    [Fact]
    public void CanContinue_ReturnsFalse_WhenSessionCannotContinue()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var roomService = new RoomTemplateService(context);
        var rewardsService = new GameRewardsService(context);
        var service = new GameSessionService(context, roomService, rewardsService);

        // Session avec santé à 0
        var session1 = new GameSession
        {
            Id = Guid.NewGuid(),
            Status = GameStatus.InProgress,
            CurrentHealth = 0,
            CurrentRoomIndex = 2,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        // Session terminée
        var session2 = new GameSession
        {
            Id = Guid.NewGuid(),
            Status = GameStatus.Completed,
            CurrentHealth = 50,
            CurrentRoomIndex = 5,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        // Session avec toutes les salles terminées
        var session3 = new GameSession
        {
            Id = Guid.NewGuid(),
            Status = GameStatus.InProgress,
            CurrentHealth = 50,
            CurrentRoomIndex = 5,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        // Act & Assert
        Assert.False(service.CanContinue(session1));
        Assert.False(service.CanContinue(session2));
        Assert.False(service.CanContinue(session3));
    }

    /// Test: GetPlayerCurrentSessionAsync retourne la session en cours d'un joueur
    [Fact]
    public async Task GetPlayerCurrentSessionAsync_ReturnsCurrentSession_WhenExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var roomService = new RoomTemplateService(context);
        var rewardsService = new GameRewardsService(context);
        var service = new GameSessionService(context, roomService, rewardsService);

        var user = new User
            { Id = Guid.NewGuid(), Username = "TestUser", Email = "test@test.com", PasswordHash = "hash" };
        context.Users.Add(user);

        var completedSession = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            StartTime = DateTime.UtcNow.AddHours(-3),
            LastSaveTime = DateTime.UtcNow.AddHours(-3),
            Status = GameStatus.Completed,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var currentSession = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            StartTime = DateTime.UtcNow.AddHours(-1),
            LastSaveTime = DateTime.UtcNow,
            Status = GameStatus.InProgress,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        context.GameSessions.AddRange(completedSession, currentSession);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPlayerCurrentSessionAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(currentSession.Id, result.Id);
        Assert.Equal(GameStatus.InProgress, result.Status);
        Assert.NotNull(result.Player);
        Assert.NotNull(result.Actions);
    }

    /// Test: GetPlayerCurrentSessionAsync retourne null quand aucune session en cours n'existe
    [Fact]
    public async Task GetPlayerCurrentSessionAsync_ReturnsNull_WhenNoCurrentSession()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var roomService = new RoomTemplateService(context);
        var rewardsService = new GameRewardsService(context);
        var service = new GameSessionService(context, roomService, rewardsService);

        var user = new User
            { Id = Guid.NewGuid(), Username = "TestUser", Email = "test@test.com", PasswordHash = "hash" };
        context.Users.Add(user);

        var completedSession = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            StartTime = DateTime.UtcNow.AddHours(-2),
            Status = GameStatus.Completed,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        context.GameSessions.Add(completedSession);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPlayerCurrentSessionAsync(user.Id);

        // Assert
        Assert.Null(result);
    }
}

