using BlazorGame.Core.Data;
using BlazorGame.Core.Services;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using SharedModels.Enums;

namespace BlazorGame.Tests;

/// <summary>
/// Tests unitaires pour le service RoomTemplateService
/// </summary>
public class RoomTemplateServiceTests
{
    /// Crée un contexte de base de données en mémoire pour les tests
    private GameDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GameDbContext(options);
    }

    /// Test: GetAllAsync retourne uniquement les templates actifs
    [Fact]
    public async Task GetAllAsync_ReturnsOnlyActiveTemplates()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);

        var activeTemplate = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Active Room",
            Description = "Active",
            Type = RoomType.Combat,
            IsActive = true
        };

        var inactiveTemplate = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Room",
            Description = "Inactive",
            Type = RoomType.Search,
            IsActive = false
        };

        context.RoomTemplates.AddRange(activeTemplate, inactiveTemplate);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(activeTemplate.Id, result[0].Id);
    }

    /// Test: GetAllAsync retourne une liste vide quand aucun template actif n'existe
    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoActiveTemplates()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    /// Test: GetByIdAsync retourne le template correspondant
    [Fact]
    public async Task GetByIdAsync_ReturnsTemplate_WhenExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);

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
        var result = await service.GetByIdAsync(template.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(template.Id, result.Id);
        Assert.Equal(template.Name, result.Name);
    }

    /// Test: GetByIdAsync retourne null quand le template n'existe pas
    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await service.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    /// Test: CreateAsync crée un nouveau template avec succès
    [Fact]
    public async Task CreateAsync_CreatesNewTemplate_Successfully()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);

        var template = new RoomTemplate
        {
            Name = "New Room",
            Description = "A new room",
            Type = RoomType.Search,
            IsActive = true
        };

        // Act
        var result = await service.CreateAsync(template);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(template.Name, result.Name);

        var savedTemplate = await context.RoomTemplates.FindAsync(result.Id);
        Assert.NotNull(savedTemplate);
    }

    /// Test: UpdateAsync met à jour un template existant
    [Fact]
    public async Task UpdateAsync_UpdatesTemplate_WhenExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);

        var original = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Original",
            Description = "Original Description",
            Type = RoomType.Combat,
            IsActive = true
        };
        context.RoomTemplates.Add(original);
        await context.SaveChangesAsync();

        var updated = new RoomTemplate
        {
            Name = "Updated",
            Description = "Updated Description",
            Type = RoomType.Search,
            IsActive = false
        };

        // Act
        var result = await service.UpdateAsync(original.Id, updated);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(RoomType.Search, result.Type);
        Assert.False(result.IsActive);
    }

    /// Test: UpdateAsync retourne null quand le template n'existe pas
    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);
        var nonExistentId = Guid.NewGuid();

        var template = new RoomTemplate
        {
            Name = "Updated",
            Description = "Test",
            Type = RoomType.Combat,
            IsActive = true
        };

        // Act
        var result = await service.UpdateAsync(nonExistentId, template);

        // Assert
        Assert.Null(result);
    }

    /// Test: DesactivateAsync désactive un template avec succès
    [Fact]
    public async Task DesactivateAsync_DesactivatesTemplate_WhenExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);

        var template = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Active Room",
            Description = "Test",
            Type = RoomType.Combat,
            IsActive = true
        };
        context.RoomTemplates.Add(template);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DesactivateAsync(template.Id);

        // Assert
        Assert.True(result);
        var updated = await context.RoomTemplates.FindAsync(template.Id);
        Assert.False(updated!.IsActive);
    }

    /// Test: DesactivateAsync retourne false quand le template n'existe pas
    [Fact]
    public async Task DesactivateAsync_ReturnsFalse_WhenNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await service.DesactivateAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    /// Test: DeleteAsync supprime un template avec succès
    [Fact]
    public async Task DeleteAsync_DeletesTemplate_WhenExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);

        var template = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "To Delete",
            Description = "Test",
            Type = RoomType.Combat,
            IsActive = true
        };
        context.RoomTemplates.Add(template);
        await context.SaveChangesAsync();

        // Act
        var result = await service.DeleteAsync(template.Id);

        // Assert
        Assert.True(result);
        var deleted = await context.RoomTemplates.FindAsync(template.Id);
        Assert.Null(deleted);
    }

    /// Test: DeleteAsync retourne false quand le template n'existe pas
    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await service.DeleteAsync(nonExistentId);

        // Assert
        Assert.False(result);
    }

    /// Test: GetAllIncludingInactiveAsync retourne tous les templates actifs et inactifs
    [Fact]
    public async Task GetAllIncludingInactiveAsync_ReturnsAllTemplates()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);

        var activeTemplate = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Active Room",
            Description = "Active",
            Type = RoomType.Combat,
            IsActive = true
        };

        var inactiveTemplate = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Room",
            Description = "Inactive",
            Type = RoomType.Search,
            IsActive = false
        };

        context.RoomTemplates.AddRange(activeTemplate, inactiveTemplate);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetAllIncludingInactiveAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Id == activeTemplate.Id);
        Assert.Contains(result, t => t.Id == inactiveTemplate.Id);
    }
}

