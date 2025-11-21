using BlazorGame.Core.Controllers;
using BlazorGame.Core.Data;
using BlazorGame.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using SharedModels.Enums;

namespace BlazorGame.Tests.Core.Controllers;

/// <summary>
/// Tests unitaires pour le controller GameSessionsController
/// </summary>
public class GameSessionsControllerTests
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
    private async Task<(GameDbContext context, GameSessionsController controller, User user)> SetupTestDataAsync()
    {
        var context = CreateInMemoryContext();

        var user = new User { Id = Guid.NewGuid() };

        var config = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 100,
            NumberOfRooms = 5,
            MinCombatVictoryPoints = 10,
            MaxCombatVictoryPoints = 20,
            MinCombatDefeatPoints = -5,
            MaxCombatDefeatPoints = 0,
            MinCombatDefeatHealthLoss = -20,
            MaxCombatDefeatHealthLoss = -10,
            MinFleePoints = -5,
            MaxFleePoints = 0
        };

        var template = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Test Room",
            Description = "Test",
            Type = RoomType.Combat,
            IsActive = true
        };

        context.Users.Add(user);
        context.GameRewards.Add(config);
        context.RoomTemplates.Add(template);
        await context.SaveChangesAsync();

        var roomService = new RoomTemplateService(context);
        var rewardsService = new GameRewardsService(context);
        var sessionService = new GameSessionService(context, roomService, rewardsService);
        var actionService = new GameActionService(context, rewardsService);

        var controller = new GameSessionsController(sessionService, actionService, roomService);

        return (context, controller, user);
    }

    /// Test: GetById retourne Ok avec la session quand elle existe
    [Fact]
    public async Task GetById_ReturnsOk_WhenSessionExists()
    {
        // Arrange
        var (context, controller, user) = await SetupTestDataAsync();

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            Score = 50,
            CurrentHealth = 100,
            Status = GameStatus.InProgress,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };
        context.GameSessions.Add(session);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetById(session.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedSession = Assert.IsType<GameSession>(okResult.Value);
        Assert.Equal(session.Id, returnedSession.Id);
    }

    /// Test: StartNewGame retourne CreatedAtAction avec la nouvelle session
    [Fact]
    public async Task StartNewGame_ReturnsCreatedAtAction_WithNewSession()
    {
        // Arrange
        var (_, controller, user) = await SetupTestDataAsync();
        var request = new StartGameRequest(user.Id);

        // Act
        var result = await controller.StartNewGame(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var session = Assert.IsType<GameSession>(createdResult.Value);
        Assert.Equal(user.Id, session.PlayerId);
        Assert.Equal(GameStatus.InProgress, session.Status);
        Assert.Equal("GetById", createdResult.ActionName);
    }

    /// Test: PerformAction retourne Ok avec la réponse d'action
    [Fact]
    public async Task PerformAction_ReturnsOk_WhenActionSucceeds()
    {
        // Arrange
        var (context, controller, user) = await SetupTestDataAsync();

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            Score = 50,
            CurrentHealth = 100,
            CurrentRoomIndex = 0,
            Status = GameStatus.InProgress,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };
        context.GameSessions.Add(session);
        await context.SaveChangesAsync();

        var request = new PerformActionRequest(ActionType.Flee);

        // Act
        var result = await controller.PerformAction(session.Id, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GameActionResponse>(okResult.Value);
        Assert.NotNull(response.Action);
        Assert.NotNull(response.UpdatedSession);
        Assert.Equal(ActionType.Flee, response.Action.Type);
    }

    /// Test: AbandonSession retourne NoContent quand l'abandon réussit
    [Fact]
    public async Task AbandonSession_ReturnsNoContent_WhenAbandonSucceeds()
    {
        // Arrange
        var (context, controller, user) = await SetupTestDataAsync();

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            Score = 50,
            CurrentHealth = 100,
            Status = GameStatus.InProgress,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };
        context.GameSessions.Add(session);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.AbandonSession(session.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    /// Test: GetSessionActions retourne Ok avec la liste des actions
    [Fact]
    public async Task GetSessionActions_ReturnsOk_WithActions()
    {
        // Arrange
        var (context, controller, user) = await SetupTestDataAsync();

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            Score = 50,
            CurrentHealth = 100,
            Status = GameStatus.InProgress,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };

        var action = new GameAction
        {
            Id = Guid.NewGuid(),
            GameSessionId = session.Id,
            Type = ActionType.Combat,
            Result = GameActionResult.Victory,
            PointsChange = 15,
            HealthChange = 0,
            RoomNumber = 1,
            Timestamp = DateTime.UtcNow
        };

        context.GameSessions.Add(session);
        context.GameActions.Add(action);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetSessionActions(session.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actions = Assert.IsType<List<GameAction>>(okResult.Value);
        Assert.Single(actions);
    }

    /// Test: GetPlayerSessions retourne Ok avec la liste des sessions
    [Fact]
    public async Task GetPlayerSessions_ReturnsOk_WithSessions()
    {
        // Arrange
        var (context, controller, user) = await SetupTestDataAsync();

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            Score = 100,
            Status = GameStatus.Completed,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };
        context.GameSessions.Add(session);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetPlayerSessions(user.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var sessions = Assert.IsType<List<GameSession>>(okResult.Value);
        Assert.Single(sessions);
    }

    /// Test: GetPlayerCurrentSession retourne Ok avec la session en cours
    [Fact]
    public async Task GetPlayerCurrentSession_ReturnsOk_WhenSessionExists()
    {
        // Arrange
        var (context, controller, user) = await SetupTestDataAsync();

        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            PlayerId = user.Id,
            Score = 50,
            Status = GameStatus.InProgress,
            TotalRooms = 5,
            GeneratedRoomsJson = "[]"
        };
        context.GameSessions.Add(session);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetPlayerCurrentSession(user.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedSession = Assert.IsType<GameSession>(okResult.Value);
        Assert.Equal(session.Id, returnedSession.Id);
    }
}

