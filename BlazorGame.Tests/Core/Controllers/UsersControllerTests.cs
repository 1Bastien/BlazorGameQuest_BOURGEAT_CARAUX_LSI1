using BlazorGame.Core.Controllers;
using BlazorGame.Core.Data;
using BlazorGame.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using SharedModels.DTOs;
using System.Security.Claims;

namespace BlazorGame.Tests;

/// <summary>
/// Tests unitaires pour le controller UsersController
/// </summary>
public class UsersControllerTests
{
    /// Crée un contexte de base de données en mémoire pour les tests
    private GameDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GameDbContext(options);
    }

    /// Crée un ControllerContext avec un utilisateur authentifié
    private ControllerContext CreateControllerContextWithUser(string userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("sub", userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    /// Test: GetAllPlayersForAdmin retourne tous les joueurs
    [Fact]
    public async Task GetAllPlayersForAdmin_ReturnsOk_WithAllPlayers()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var controller = new UsersController(userService);

        var user1 = new User { Id = Guid.NewGuid(), Username = "player1" };
        var user2 = new User { Id = Guid.NewGuid(), Username = "player2" };
        context.Users.AddRange(user1, user2);

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user1.Id,
            StartTime = DateTime.UtcNow,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };
        context.GameSessions.Add(session);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetAllPlayersForAdmin();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var players = Assert.IsAssignableFrom<List<PlayerAdminDto>>(okResult.Value);
        Assert.Equal(2, players.Count);
    }

    /// Test: GetAllPlayersForAdmin retourne une liste vide quand aucun joueur n'existe
    [Fact]
    public async Task GetAllPlayersForAdmin_ReturnsEmptyList_WhenNoPlayers()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var controller = new UsersController(userService);

        // Act
        var result = await controller.GetAllPlayersForAdmin();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var players = Assert.IsAssignableFrom<List<PlayerAdminDto>>(okResult.Value);
        Assert.Empty(players);
    }

    /// Test: GetPlayerSessions retourne toutes les sessions d'un joueur
    [Fact]
    public async Task GetPlayerSessions_ReturnsOk_WithPlayerSessions()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var controller = new UsersController(userService);

        var user = new User { Id = Guid.NewGuid() };
        context.Users.Add(user);

        var session1 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            StartTime = DateTime.UtcNow.AddHours(-2),
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var session2 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            StartTime = DateTime.UtcNow.AddHours(-1),
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        context.GameSessions.AddRange(session1, session2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetPlayerSessions(user.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var sessions = Assert.IsAssignableFrom<List<GameSession>>(okResult.Value);
        Assert.Equal(2, sessions.Count);
        Assert.All(sessions, s => Assert.Equal(user.Id, s.PlayerId));
    }

    /// Test: GetPlayerSessions retourne une liste vide si le joueur n'a pas de sessions
    [Fact]
    public async Task GetPlayerSessions_ReturnsEmptyList_WhenNoSessions()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var controller = new UsersController(userService);

        var user = new User { Id = Guid.NewGuid() };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetPlayerSessions(user.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var sessions = Assert.IsAssignableFrom<List<GameSession>>(okResult.Value);
        Assert.Empty(sessions);
    }

    /// Test: DeletePlayerSessions supprime toutes les sessions et retourne NoContent
    [Fact]
    public async Task DeletePlayerSessions_ReturnsNoContent_WhenSessionsDeleted()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var controller = new UsersController(userService);

        var user = new User { Id = Guid.NewGuid() };
        context.Users.Add(user);

        var session1 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var session2 = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        context.GameSessions.AddRange(session1, session2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.DeletePlayerSessions(user.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        var remainingSessions = await context.GameSessions.Where(s => s.PlayerId == user.Id).ToListAsync();
        Assert.Empty(remainingSessions);
    }

    /// Test: DeletePlayerSessions retourne NotFound si le joueur n'a pas de sessions
    [Fact]
    public async Task DeletePlayerSessions_ReturnsNotFound_WhenNoSessions()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var controller = new UsersController(userService);

        var user = new User { Id = Guid.NewGuid() };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.DeletePlayerSessions(user.Id);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Aucune session trouvée pour ce joueur", notFoundResult.Value);
    }

    /// Test: GetMyStatus retourne le statut de l'utilisateur connecté
    [Fact]
    public async Task GetMyStatus_ReturnsUserStatus_WhenUserExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var controller = new UsersController(userService);

        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Simuler un utilisateur authentifié
        controller.ControllerContext = CreateControllerContextWithUser(userId.ToString());

        // Act
        var result = await controller.GetMyStatus();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var status = Assert.IsType<UserStatusDto>(okResult.Value);
        Assert.Equal(userId, status.UserId);
        Assert.True(status.IsActive);
    }

    /// Test: GetMyStatus retourne IsActive=true si l'utilisateur n'existe pas encore
    [Fact]
    public async Task GetMyStatus_ReturnsActiveTrue_WhenUserDoesNotExist()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var controller = new UsersController(userService);

        var userId = Guid.NewGuid();

        // Simuler un utilisateur authentifié
        controller.ControllerContext = CreateControllerContextWithUser(userId.ToString());

        // Act
        var result = await controller.GetMyStatus();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var status = Assert.IsType<UserStatusDto>(okResult.Value);
        Assert.Equal(userId, status.UserId);
        Assert.True(status.IsActive); // Par défaut, un nouvel utilisateur est actif
    }

    /// Test: ToggleUserActiveStatus désactive un utilisateur actif
    [Fact]
    public async Task ToggleUserActiveStatus_DeactivatesUser_WhenActive()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var controller = new UsersController(userService);

        var user = new User { Id = Guid.NewGuid(), IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.ToggleUserActiveStatus(user.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var updatedUser = Assert.IsType<User>(okResult.Value);
        Assert.False(updatedUser.IsActive);
    }

    /// Test: ToggleUserActiveStatus active un utilisateur désactivé
    [Fact]
    public async Task ToggleUserActiveStatus_ActivatesUser_WhenInactive()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var controller = new UsersController(userService);

        var user = new User { Id = Guid.NewGuid(), IsActive = false };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.ToggleUserActiveStatus(user.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var updatedUser = Assert.IsType<User>(okResult.Value);
        Assert.True(updatedUser.IsActive);
    }

    /// Test: ToggleUserActiveStatus retourne NotFound si l'utilisateur n'existe pas
    [Fact]
    public async Task ToggleUserActiveStatus_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var controller = new UsersController(userService);

        // Act
        var result = await controller.ToggleUserActiveStatus(Guid.NewGuid());

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Utilisateur non trouvé", notFoundResult.Value);
    }
}

