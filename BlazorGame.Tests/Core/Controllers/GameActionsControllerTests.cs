using BlazorGame.Core.Controllers;
using BlazorGame.Core.Data;
using BlazorGame.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using SharedModels.Enums;

namespace BlazorGame.Tests.Core.Controllers;

/// <summary>
/// Tests unitaires pour le controller GameActionsController
/// </summary>
public class GameActionsControllerTests
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
    private async Task<(GameDbContext context, GameActionsController controller, GameSession session)>
        SetupTestDataAsync()
    {
        var context = CreateInMemoryContext();

        var user = new User { Id = Guid.NewGuid() };

        var config = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 100,
            MinCombatVictoryPoints = 10,
            MaxCombatVictoryPoints = 20
        };

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

        context.Users.Add(user);
        context.GameRewards.Add(config);
        context.GameSessions.Add(session);
        await context.SaveChangesAsync();

        var rewardsService = new GameRewardsService(context);
        var service = new GameActionService(context, rewardsService);
        var controller = new GameActionsController(service);

        return (context, controller, session);
    }

    /// Test: GetAll retourne Ok avec toutes les actions
    [Fact]
    public async Task GetAll_ReturnsOk_WithAllActions()
    {
        // Arrange
        var (context, controller, session) = await SetupTestDataAsync();

        var action1 = new GameAction
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

        var action2 = new GameAction
        {
            Id = Guid.NewGuid(),
            GameSessionId = session.Id,
            Type = ActionType.Search,
            Result = GameActionResult.FoundTreasure,
            PointsChange = 20,
            HealthChange = 0,
            RoomNumber = 2,
            Timestamp = DateTime.UtcNow
        };

        context.GameActions.AddRange(action1, action2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actions = Assert.IsType<List<GameAction>>(okResult.Value);
        Assert.Equal(2, actions.Count);
    }

    /// Test: GetById retourne Ok avec l'action quand elle existe
    [Fact]
    public async Task GetById_ReturnsOk_WhenActionExists()
    {
        // Arrange
        var (context, controller, session) = await SetupTestDataAsync();

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

        context.GameActions.Add(action);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetById(action.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAction = Assert.IsType<GameAction>(okResult.Value);
        Assert.Equal(action.Id, returnedAction.Id);
    }

    /// Test: GetById retourne NotFound quand l'action n'existe pas
    [Fact]
    public async Task GetById_ReturnsNotFound_WhenActionNotExists()
    {
        // Arrange
        var (_, controller, _) = await SetupTestDataAsync();

        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await controller.GetById(nonExistentId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    /// Test: Delete retourne NoContent quand la suppression réussit
    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleteSucceeds()
    {
        // Arrange
        var (context, controller, session) = await SetupTestDataAsync();

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

        context.GameActions.Add(action);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.Delete(action.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    /// Test: Delete retourne NotFound quand l'action n'existe pas
    [Fact]
    public async Task Delete_ReturnsNotFound_WhenActionNotExists()
    {
        // Arrange
        var (_, controller, _) = await SetupTestDataAsync();

        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await controller.Delete(nonExistentId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    /// Test: Create retourne CreatedAtAction avec l'action créée
    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WithCreatedAction()
    {
        // Arrange
        var (_, controller, session) = await SetupTestDataAsync();

        var newAction = new GameAction
        {
            GameSessionId = session.Id,
            Type = ActionType.Combat,
            Result = GameActionResult.Victory,
            PointsChange = 15,
            HealthChange = 0,
            RoomNumber = 1
        };

        // Act
        var result = await controller.Create(newAction);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var action = Assert.IsType<GameAction>(createdResult.Value);
        Assert.NotEqual(Guid.Empty, action.Id);
        Assert.Equal(ActionType.Combat, action.Type);
        Assert.Equal("GetById", createdResult.ActionName);
    }

    /// Test: Update retourne Ok avec l'action mise à jour
    [Fact]
    public async Task Update_ReturnsOk_WhenUpdateSucceeds()
    {
        // Arrange
        var (context, controller, session) = await SetupTestDataAsync();

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
        context.GameActions.Add(action);
        await context.SaveChangesAsync();

        var updatedAction = new GameAction
        {
            Type = ActionType.Search,
            Result = GameActionResult.FoundTreasure,
            PointsChange = 25,
            HealthChange = 0,
            RoomNumber = 2
        };

        // Act
        var result = await controller.Update(action.Id, updatedAction);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAction = Assert.IsType<GameAction>(okResult.Value);
        Assert.Equal(ActionType.Search, returnedAction.Type);
        Assert.Equal(GameActionResult.FoundTreasure, returnedAction.Result);
        Assert.Equal(25, returnedAction.PointsChange);
    }

    /// Test: Update retourne NotFound quand l'action n'existe pas
    [Fact]
    public async Task Update_ReturnsNotFound_WhenActionNotExists()
    {
        // Arrange
        var (_, controller, _) = await SetupTestDataAsync();

        var nonExistentId = Guid.NewGuid();
        var action = new GameAction
        {
            Type = ActionType.Combat,
            Result = GameActionResult.Victory,
            PointsChange = 15,
            HealthChange = 0,
            RoomNumber = 1
        };

        // Act
        var result = await controller.Update(nonExistentId, action);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
}

