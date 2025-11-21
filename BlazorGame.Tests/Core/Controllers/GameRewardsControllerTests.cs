using BlazorGame.Core.Controllers;
using BlazorGame.Core.Data;
using BlazorGame.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;

namespace BlazorGame.Tests.Core.Controllers;

/// <summary>
/// Tests unitaires pour le controller GameRewardsController
/// </summary>
public class GameRewardsControllerTests
{
    /// Crée un contexte de base de données en mémoire pour les tests
    private GameDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GameDbContext(options);
    }

    /// Test: GetConfig retourne Ok avec la configuration quand elle existe
    [Fact]
    public async Task GetConfig_ReturnsOk_WhenConfigExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);
        var controller = new GameRewardsController(service);

        var expectedConfig = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 100,
            MinCombatVictoryPoints = 10,
            MaxCombatVictoryPoints = 20
        };
        context.GameRewards.Add(expectedConfig);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetConfig();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var config = Assert.IsType<GameRewards>(okResult.Value);
        Assert.Equal(expectedConfig.Id, config.Id);
        Assert.Equal(expectedConfig.StartingHealth, config.StartingHealth);
    }

    /// Test: UpdateConfig retourne Ok avec la configuration mise à jour
    [Fact]
    public async Task UpdateConfig_ReturnsOk_WhenUpdateSucceeds()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);
        var controller = new GameRewardsController(service);

        var originalConfig = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 100
        };
        context.GameRewards.Add(originalConfig);
        await context.SaveChangesAsync();

        var updatedConfig = new GameRewards
        {
            Id = originalConfig.Id,
            StartingHealth = 150
        };

        // Act
        var result = await controller.UpdateConfig(updatedConfig);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var config = Assert.IsType<GameRewards>(okResult.Value);
        Assert.Equal(150, config.StartingHealth);
    }

    /// Test: GetConfig retourne NotFound quand la configuration n'existe pas
    [Fact]
    public async Task GetConfig_ReturnsNotFound_WhenConfigNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);
        var controller = new GameRewardsController(service);

        // Act
        var result = await controller.GetConfig();

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    /// Test: UpdateConfig retourne NotFound quand la configuration n'existe pas
    [Fact]
    public async Task UpdateConfig_ReturnsNotFound_WhenConfigNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);
        var controller = new GameRewardsController(service);

        var config = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 150
        };

        // Act
        var result = await controller.UpdateConfig(config);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    /// Test: CreateConfig retourne CreatedAtAction avec la configuration créée
    [Fact]
    public async Task CreateConfig_ReturnsCreatedAtAction_WithCreatedConfig()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);
        var controller = new GameRewardsController(service);

        var newConfig = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 120,
            MinCombatVictoryPoints = 15,
            MaxCombatVictoryPoints = 25
        };

        // Act
        var result = await controller.CreateConfig(newConfig);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var config = Assert.IsType<GameRewards>(createdResult.Value);
        Assert.Equal(newConfig.Id, config.Id);
        Assert.Equal(120, config.StartingHealth);
        Assert.Equal("GetConfig", createdResult.ActionName);
    }

    /// Test: DeleteConfig retourne NoContent quand la suppression réussit
    [Fact]
    public async Task DeleteConfig_ReturnsNoContent_WhenDeleteSucceeds()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);
        var controller = new GameRewardsController(service);

        var config = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 100
        };
        context.GameRewards.Add(config);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.DeleteConfig();

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    /// Test: DeleteConfig retourne NotFound quand la configuration n'existe pas
    [Fact]
    public async Task DeleteConfig_ReturnsNotFound_WhenConfigNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);
        var controller = new GameRewardsController(service);

        // Act
        var result = await controller.DeleteConfig();

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    /// Test: GetAll retourne Ok avec toutes les configurations
    [Fact]
    public async Task GetAll_ReturnsOk_WithAllConfigs()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);
        var controller = new GameRewardsController(service);

        var config1 = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 100
        };

        var config2 = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 150
        };

        context.GameRewards.AddRange(config1, config2);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var configs = Assert.IsType<List<GameRewards>>(okResult.Value);
        Assert.Equal(2, configs.Count);
    }
}

