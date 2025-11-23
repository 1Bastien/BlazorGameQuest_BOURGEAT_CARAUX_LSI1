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

    /// Test: GetAllPlayersForAdminAsync retourne tous les joueurs avec leurs statistiques
    [Fact]
    public async Task GetAllPlayersForAdminAsync_ReturnsAllPlayers_WithStatistics()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        var user1 = new User { Id = Guid.NewGuid(), Username = "player1" };
        var user2 = new User { Id = Guid.NewGuid(), Username = "player2" };
        var user3 = new User { Id = Guid.NewGuid(), Username = "player3" };
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
        var result = await service.GetAllPlayersForAdminAsync();

        // Assert
        Assert.Equal(3, result.Count);

        // Vérifier que user2 est en premier (dernière connexion la plus récente)
        Assert.Equal(user2.Id, result[0].UserId);
        Assert.Equal("player2", result[0].Username);
        Assert.Equal(1, result[0].TotalGamesPlayed);
        Assert.NotNull(result[0].LastConnectionDate);

        // Vérifier user1
        var user1Result = result.First(p => p.UserId == user1.Id);
        Assert.Equal(2, user1Result.TotalGamesPlayed);

        // Vérifier user3 (sans parties)
        var user3Result = result.First(p => p.UserId == user3.Id);
        Assert.Equal(0, user3Result.TotalGamesPlayed);
        Assert.Null(user3Result.LastConnectionDate);
    }

    /// Test: GetAllPlayersForAdminAsync trie correctement les joueurs
    [Fact]
    public async Task GetAllPlayersForAdminAsync_SortsPlayers_ByLastConnectionAndUsername()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        var userWithGames = new User { Id = Guid.NewGuid(), Username = "activePlayer" };
        var userNoGames1 = new User { Id = Guid.NewGuid(), Username = "zzzInactive" };
        var userNoGames2 = new User { Id = Guid.NewGuid(), Username = "aaaInactive" };
        context.Users.AddRange(userWithGames, userNoGames1, userNoGames2);

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = userWithGames.Id,
            StartTime = DateTime.UtcNow,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };
        context.GameSessions.Add(session);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllPlayersForAdminAsync();

        // Assert
        Assert.Equal(3, result.Count);
        // Premier: joueur avec parties
        Assert.Equal(userWithGames.Id, result[0].UserId);
        // Suivants: joueurs sans parties triés par nom
        Assert.Equal("aaaInactive", result[1].Username);
        Assert.Equal("zzzInactive", result[2].Username);
    }

    /// Test: GetPlayerSessionsAsync retourne toutes les sessions d'un joueur
    [Fact]
    public async Task GetPlayerSessionsAsync_ReturnsAllSessions_ForPlayer()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        var user1 = new User { Id = Guid.NewGuid() };
        var user2 = new User { Id = Guid.NewGuid() };
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
            StartTime = DateTime.UtcNow.AddHours(-1),
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var session3 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user2.Id,
            StartTime = DateTime.UtcNow,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        context.GameSessions.AddRange(session1, session2, session3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPlayerSessionsAsync(user1.Id);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, s => Assert.Equal(user1.Id, s.PlayerId));
        // Vérifier le tri par date décroissante
        Assert.True(result[0].StartTime > result[1].StartTime);
    }

    /// Test: GetPlayerSessionsAsync retourne une liste vide si le joueur n'a pas de sessions
    [Fact]
    public async Task GetPlayerSessionsAsync_ReturnsEmptyList_WhenNoSessions()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        var user = new User { Id = Guid.NewGuid() };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetPlayerSessionsAsync(user.Id);

        // Assert
        Assert.Empty(result);
    }

    /// Test: DeletePlayerSessionsAsync supprime toutes les sessions d'un joueur
    [Fact]
    public async Task DeletePlayerSessionsAsync_DeletesAllSessions_ForPlayer()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        var user1 = new User { Id = Guid.NewGuid() };
        var user2 = new User { Id = Guid.NewGuid() };
        context.Users.AddRange(user1, user2);

        var session1 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user1.Id,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var session2 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user1.Id,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var session3 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user2.Id,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        context.GameSessions.AddRange(session1, session2, session3);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeletePlayerSessionsAsync(user1.Id);

        // Assert
        Assert.True(result);
        var remainingSessions = await context.GameSessions.ToListAsync();
        Assert.Single(remainingSessions);
        Assert.Equal(user2.Id, remainingSessions[0].PlayerId);
    }

    /// Test: DeletePlayerSessionsAsync retourne false si le joueur n'a pas de sessions
    [Fact]
    public async Task DeletePlayerSessionsAsync_ReturnsFalse_WhenNoSessions()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        var user = new User { Id = Guid.NewGuid() };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeletePlayerSessionsAsync(user.Id);

        // Assert
        Assert.False(result);
    }

    /// Test: ToggleUserActiveStatusAsync désactive un utilisateur actif
    [Fact]
    public async Task ToggleUserActiveStatusAsync_DeactivatesUser_WhenActive()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        var user = new User { Id = Guid.NewGuid(), IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ToggleUserActiveStatusAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsActive);

        var userInDb = await context.Users.FindAsync(user.Id);
        Assert.False(userInDb!.IsActive);
    }

    /// Test: ToggleUserActiveStatusAsync active un utilisateur désactivé
    [Fact]
    public async Task ToggleUserActiveStatusAsync_ActivatesUser_WhenInactive()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        var user = new User { Id = Guid.NewGuid(), IsActive = false };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await service.ToggleUserActiveStatusAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsActive);

        var userInDb = await context.Users.FindAsync(user.Id);
        Assert.True(userInDb!.IsActive);
    }

    /// Test: ToggleUserActiveStatusAsync retourne null si l'utilisateur n'existe pas
    [Fact]
    public async Task ToggleUserActiveStatusAsync_ReturnsNull_WhenUserNotFound()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        // Act
        var result = await service.ToggleUserActiveStatusAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    /// Test: GetAllPlayersForAdminAsync inclut le statut IsActive
    [Fact]
    public async Task GetAllPlayersForAdminAsync_IncludesIsActiveStatus()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        var activeUser = new User { Id = Guid.NewGuid(), Username = "active", IsActive = true };
        var inactiveUser = new User { Id = Guid.NewGuid(), Username = "inactive", IsActive = false };
        context.Users.AddRange(activeUser, inactiveUser);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllPlayersForAdminAsync();

        // Assert
        Assert.Equal(2, result.Count);

        var activePlayerDto = result.First(p => p.UserId == activeUser.Id);
        Assert.True(activePlayerDto.IsActive);

        var inactivePlayerDto = result.First(p => p.UserId == inactiveUser.Id);
        Assert.False(inactivePlayerDto.IsActive);
    }

    /// Test: EnsureUserExistsAsync crée un utilisateur avec un rôle
    [Fact]
    public async Task EnsureUserExistsAsync_CreatesUser_WithRole()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);
        var userId = Guid.NewGuid();

        // Act
        var result = await service.EnsureUserExistsAsync(userId, "testuser", "administrateur");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("administrateur", result.Role);
    }

    /// Test: EnsureUserExistsAsync met à jour le rôle si changé
    [Fact]
    public async Task EnsureUserExistsAsync_UpdatesRole_WhenChanged()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);
        var userId = Guid.NewGuid();

        var existingUser = new User { Id = userId, Username = "testuser", Role = "joueur" };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        // Act
        var result = await service.EnsureUserExistsAsync(userId, "testuser", "administrateur");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("administrateur", result.Role);

        var userInDb = await context.Users.FindAsync(userId);
        Assert.Equal("administrateur", userInDb!.Role);
    }

    /// Test: GetAllPlayersForAdminAsync inclut le rôle
    [Fact]
    public async Task GetAllPlayersForAdminAsync_IncludesRole()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new UserService(context);

        var admin = new User { Id = Guid.NewGuid(), Username = "admin", Role = "administrateur" };
        var player = new User { Id = Guid.NewGuid(), Username = "player", Role = "joueur" };
        context.Users.AddRange(admin, player);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllPlayersForAdminAsync();

        // Assert
        Assert.Equal(2, result.Count);

        var adminDto = result.First(p => p.UserId == admin.Id);
        Assert.Equal("administrateur", adminDto.Role);

        var playerDto = result.First(p => p.UserId == player.Id);
        Assert.Equal("joueur", playerDto.Role);
    }
}

