using BlazorGame.Core.Data;
using BlazorGame.Core.Services;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;

namespace BlazorGame.Tests;

/// <summary>
/// Tests unitaires pour le service GameRewardsService
/// </summary>
public class GameRewardsServiceTests
{
    /// Crée un contexte de base de données en mémoire pour les tests
    private GameDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GameDbContext(options);
    }

    /// Test GetConfigAsync retourne la configuration existante
    [Fact]
    public async Task GetConfigAsync_ReturnsConfig_WhenExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);

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
        var result = await service.GetConfigAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedConfig.Id, result.Id);
        Assert.Equal(expectedConfig.StartingHealth, result.StartingHealth);
    }

    /// Test GetConfigAsync retourne null quand aucune configuration n'existe
    [Fact]
    public async Task GetConfigAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);

        // Act
        var result = await service.GetConfigAsync();

        // Assert
        Assert.Null(result);
    }

    /// Test UpdateConfigAsync met à jour la configuration existante
    [Fact]
    public async Task UpdateConfigAsync_UpdatesConfig_WhenExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);

        var originalConfig = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 100,
        };
        context.GameRewards.Add(originalConfig);
        await context.SaveChangesAsync();

        var updatedConfig = new GameRewards
        {
            Id = originalConfig.Id,
            StartingHealth = 150,
        };

        // Act
        var result = await service.UpdateConfigAsync(updatedConfig);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(150, result.StartingHealth);
    }

    /// Test UpdateConfigAsync lève une exception quand la configuration n'existe pas
    [Fact]
    public async Task UpdateConfigAsync_ThrowsException_WhenNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);

        var config = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 150
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateConfigAsync(config)
        );
    }

    /// Test GetRandomInRange retourne une valeur dans la plage spécifiée
    [Fact]
    public void GetRandomInRange_ReturnsValueInRange()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);
        int min = 10;
        int max = 20;

        // Act
        var result = service.GetRandomInRange(min, max);

        // Assert
        Assert.InRange(result, min, max);
    }

    /// Test GetRandomInRange avec min égal à max retourne cette valeur
    [Fact]
    public void GetRandomInRange_ReturnsExactValue_WhenMinEqualsMax()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);
        int value = 15;

        // Act
        var result = service.GetRandomInRange(value, value);

        // Assert
        Assert.Equal(value, result);
    }

    /// Test CreateConfigAsync crée une nouvelle configuration
    [Fact]
    public async Task CreateConfigAsync_CreatesConfig_Successfully()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);

        var newConfig = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 120,
            MinCombatVictoryPoints = 15,
            MaxCombatVictoryPoints = 25
        };

        // Act
        var result = await service.CreateConfigAsync(newConfig);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newConfig.Id, result.Id);
        Assert.Equal(120, result.StartingHealth);

        var savedConfig = await context.GameRewards.FindAsync(newConfig.Id);
        Assert.NotNull(savedConfig);
    }

    /// Test DeleteConfigAsync supprime la configuration
    [Fact]
    public async Task DeleteConfigAsync_DeletesConfig_WhenExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);

        var config = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 100
        };
        context.GameRewards.Add(config);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteConfigAsync();

        // Assert
        Assert.True(result);
        var deleted = await context.GameRewards.FindAsync(config.Id);
        Assert.Null(deleted);
    }

    /// Test DeleteConfigAsync retourne false quand aucune configuration n'existe
    [Fact]
    public async Task DeleteConfigAsync_ReturnsFalse_WhenNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);

        // Act
        var result = await service.DeleteConfigAsync();

        // Assert
        Assert.False(result);
    }

    /// Test GetAllAsync retourne toutes les configurations
    [Fact]
    public async Task GetAllAsync_ReturnsAllConfigs()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new GameRewardsService(context);

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
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Id == config1.Id);
        Assert.Contains(result, c => c.Id == config2.Id);
    }
}

