using BlazorGame.Core.Controllers;
using BlazorGame.Core.Data;
using BlazorGame.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using SharedModels.DTOs;

namespace BlazorGame.Tests;

/// <summary>
/// Tests unitaires pour le controller LeaderboardController
/// </summary>
public class LeaderboardControllerTests
{
    /// Crée un contexte de base de données en mémoire pour les tests
    private GameDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GameDbContext(options);
    }

    /// Test: GetLeaderboard retourne le classement avec succès
    [Fact]
    public async Task GetLeaderboard_ReturnsOk_WithLeaderboard()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var controller = new LeaderboardController(userService);

        var user1 = new User { Id = Guid.NewGuid() };
        var user2 = new User { Id = Guid.NewGuid() };
        context.Users.AddRange(user1, user2);

        var session1 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user1.Id,
            Score = 150,
            Status = SharedModels.Enums.GameStatus.Completed,
            StartTime = DateTime.UtcNow,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var session2 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user2.Id,
            Score = 100,
            Status = SharedModels.Enums.GameStatus.Completed,
            StartTime = DateTime.UtcNow,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        context.GameSessions.AddRange(session1, session2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetLeaderboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var leaderboard = Assert.IsAssignableFrom<List<LeaderboardEntry>>(okResult.Value);
        Assert.Equal(2, leaderboard.Count);
        Assert.Equal(user1.Id, leaderboard[0].UserId);
        Assert.Equal(150, leaderboard[0].TotalScore);
    }

    /// Test: GetLeaderboard retourne une liste vide quand aucune session n'existe
    [Fact]
    public async Task GetLeaderboard_ReturnsEmptyList_WhenNoSessions()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var controller = new LeaderboardController(userService);

        var user = new User { Id = Guid.NewGuid() };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetLeaderboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var leaderboard = Assert.IsAssignableFrom<List<LeaderboardEntry>>(okResult.Value);
        Assert.Empty(leaderboard);
    }

    /// Test: GetLeaderboard retourne le classement trié par score décroissant
    [Fact]
    public async Task GetLeaderboard_ReturnsLeaderboard_OrderedByScoreDesc()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var controller = new LeaderboardController(userService);

        var user1 = new User { Id = Guid.NewGuid() };
        var user2 = new User { Id = Guid.NewGuid() };
        var user3 = new User { Id = Guid.NewGuid() };
        context.Users.AddRange(user1, user2, user3);

        var session1 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user1.Id,
            Score = 50,
            Status = SharedModels.Enums.GameStatus.Completed,
            StartTime = DateTime.UtcNow,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var session2 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user2.Id,
            Score = 200,
            Status = SharedModels.Enums.GameStatus.Completed,
            StartTime = DateTime.UtcNow,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var session3 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user3.Id,
            Score = 100,
            Status = SharedModels.Enums.GameStatus.Completed,
            StartTime = DateTime.UtcNow,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        context.GameSessions.AddRange(session1, session2, session3);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetLeaderboard();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var leaderboard = Assert.IsAssignableFrom<List<LeaderboardEntry>>(okResult.Value);
        Assert.Equal(3, leaderboard.Count);
        Assert.Equal(user2.Id, leaderboard[0].UserId);
        Assert.Equal(200, leaderboard[0].TotalScore);
        Assert.Equal(user3.Id, leaderboard[1].UserId);
        Assert.Equal(100, leaderboard[1].TotalScore);
        Assert.Equal(user1.Id, leaderboard[2].UserId);
        Assert.Equal(50, leaderboard[2].TotalScore);
    }
}

