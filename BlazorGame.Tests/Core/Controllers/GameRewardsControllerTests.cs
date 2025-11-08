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
}

