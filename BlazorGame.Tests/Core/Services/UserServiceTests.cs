using BlazorGame.Core.Data;
using BlazorGame.Core.Services;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;

namespace BlazorGame.Tests;

/// <summary>
/// Tests unitaires pour le service UserService
/// </summary>
public class UserServiceTests
{
    /// Crée un contexte de base de données en mémoire pour les tests
    private GameDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GameDbContext(options);
    }

    /// Test: EnsureUserExistsAsync crée un utilisateur s'il n'existe pas
    [Fact]
    public async Task EnsureUserExistsAsync_CreatesUser_WhenNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);
        var userId = Guid.NewGuid();

        // Act
        var result = await service.EnsureUserExistsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        var userInDb = await context.Users.FindAsync(userId);
        Assert.NotNull(userInDb);
    }

    /// Test: EnsureUserExistsAsync retourne l'utilisateur existant
    [Fact]
    public async Task EnsureUserExistsAsync_ReturnsExistingUser_WhenExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        // Act
        var result = await service.EnsureUserExistsAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(1, await context.Users.CountAsync());
    }

    /// Test: GetAllAsync retourne tous les utilisateurs avec leurs sessions
    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers_WithSessions()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        var user1 = new User { Id = Guid.NewGuid() };
        var user2 = new User { Id = Guid.NewGuid() };
        context.Users.AddRange(user1, user2);

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user1.Id,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };
        context.GameSessions.Add(session);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, u => u.Id == user1.Id);
        Assert.Contains(result, u => u.Id == user2.Id);
        var userWithSession = result.First(u => u.Id == user1.Id);
        Assert.NotNull(userWithSession.GameSessions);
        Assert.Single(userWithSession.GameSessions);
    }

    /// Test: GetLeaderboardAsync retourne le classement trié par score total
    [Fact]
    public async Task GetLeaderboardAsync_ReturnsLeaderboard_OrderedByTotalScore()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        var user1 = new User { Id = Guid.NewGuid() };
        var user2 = new User { Id = Guid.NewGuid() };
        var user3 = new User { Id = Guid.NewGuid() };
        context.Users.AddRange(user1, user2, user3);

        var session1 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user1.Id,
            Score = 100,
            Status = SharedModels.Enums.GameStatus.Completed,
            StartTime = DateTime.UtcNow.AddHours(-2),
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var session2 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user1.Id,
            Score = 50,
            Status = SharedModels.Enums.GameStatus.Failed,
            StartTime = DateTime.UtcNow.AddHours(-1),
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var session3 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user2.Id,
            Score = 200,
            Status = SharedModels.Enums.GameStatus.Completed,
            StartTime = DateTime.UtcNow,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        context.GameSessions.AddRange(session1, session2, session3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetLeaderboardAsync();

        // Assert
        Assert.Equal(2, result.Count); // user3 n'a pas de sessions
        Assert.Equal(user2.Id, result[0].UserId); // 200 points
        Assert.Equal(200, result[0].TotalScore);
        Assert.Equal(1, result[0].TotalSessions);
        Assert.Equal(user1.Id, result[1].UserId); // 150 points
        Assert.Equal(150, result[1].TotalScore);
        Assert.Equal(2, result[1].TotalSessions);
    }

    /// Test: GetLeaderboardAsync retourne une liste vide quand aucun utilisateur n'a de sessions
    [Fact]
    public async Task GetLeaderboardAsync_ReturnsEmptyList_WhenNoSessions()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        var user1 = new User { Id = Guid.NewGuid() };
        var user2 = new User { Id = Guid.NewGuid() };
        context.Users.AddRange(user1, user2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetLeaderboardAsync();

        // Assert
        Assert.Empty(result);
    }

    /// Test: GetLeaderboardAsync retourne la date de la dernière session
    [Fact]
    public async Task GetLeaderboardAsync_ReturnsLastSessionDate()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        var user = new User { Id = Guid.NewGuid() };
        context.Users.Add(user);

        var oldSession = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            Score = 50,
            Status = SharedModels.Enums.GameStatus.Completed,
            StartTime = DateTime.UtcNow.AddDays(-5),
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var recentSession = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            Score = 100,
            Status = SharedModels.Enums.GameStatus.Completed,
            StartTime = DateTime.UtcNow.AddHours(-1),
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        context.GameSessions.AddRange(oldSession, recentSession);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetLeaderboardAsync();

        // Assert
        Assert.Single(result);
        Assert.NotNull(result[0].LastSessionDate);
        Assert.True(result[0].LastSessionDate > DateTime.UtcNow.AddHours(-2));
    }

    /// Test: GetLeaderboardAsync exclut les parties en cours et abandonnées
    [Fact]
    public async Task GetLeaderboardAsync_ExcludesInProgressAndAbandonedSessions()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        var user = new User { Id = Guid.NewGuid() };
        context.Users.Add(user);

        var completedSession = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            Score = 100,
            Status = SharedModels.Enums.GameStatus.Completed,
            StartTime = DateTime.UtcNow,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var inProgressSession = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            Score = 50,
            Status = SharedModels.Enums.GameStatus.InProgress,
            StartTime = DateTime.UtcNow,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var abandonedSession = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            Score = 75,
            Status = SharedModels.Enums.GameStatus.Abandoned,
            StartTime = DateTime.UtcNow,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        context.GameSessions.AddRange(completedSession, inProgressSession, abandonedSession);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetLeaderboardAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(100, result[0].TotalScore); // Seulement la session completed
        Assert.Equal(1, result[0].TotalSessions);
    }
}

