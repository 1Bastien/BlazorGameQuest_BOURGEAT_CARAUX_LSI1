using BlazorGame.Core.Controllers;
using BlazorGame.Core.Data;
using BlazorGame.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Entities;
using SharedModels.Enums;

namespace BlazorGame.Tests.Core.Controllers;

/// <summary>
/// Tests unitaires pour le controller RoomTemplatesController
/// </summary>
public class RoomTemplatesControllerTests
{
    /// Crée un contexte de base de données en mémoire pour les tests
    private GameDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GameDbContext(options);
    }

    /// Test: GetAll retourne Ok avec la liste des templates actifs
    [Fact]
    public async Task GetAll_ReturnsOk_WithActiveTemplates()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);
        var controller = new RoomTemplatesController(service);

        var activeTemplate = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Active Room",
            Description = "Test",
            Type = RoomType.Combat,
            IsActive = true
        };

        var inactiveTemplate = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Room",
            Description = "Test",
            Type = RoomType.Search,
            IsActive = false
        };

        context.RoomTemplates.AddRange(activeTemplate, inactiveTemplate);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var templates = Assert.IsType<List<RoomTemplate>>(okResult.Value);
        Assert.Single(templates);
        Assert.Equal(activeTemplate.Id, templates[0].Id);
    }

    /// Test: GetAllForAdmin retourne Ok avec tous les templates
    [Fact]
    public async Task GetAllForAdmin_ReturnsOk_WithAllTemplates()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);
        var controller = new RoomTemplatesController(service);

        var activeTemplate = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Active Room",
            Description = "Test",
            Type = RoomType.Combat,
            IsActive = true
        };

        var inactiveTemplate = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Room",
            Description = "Test",
            Type = RoomType.Search,
            IsActive = false
        };

        context.RoomTemplates.AddRange(activeTemplate, inactiveTemplate);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetAllForAdmin();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var templates = Assert.IsType<List<RoomTemplate>>(okResult.Value);
        Assert.Equal(2, templates.Count);
    }

    /// Test: GetById retourne Ok avec le template quand il existe
    [Fact]
    public async Task GetById_ReturnsOk_WhenTemplateExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);
        var controller = new RoomTemplatesController(service);

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
        var result = await controller.GetById(template.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTemplate = Assert.IsType<RoomTemplate>(okResult.Value);
        Assert.Equal(template.Id, returnedTemplate.Id);
    }

    /// Test: Create retourne CreatedAtAction avec le template créé
    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WithCreatedTemplate()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);
        var controller = new RoomTemplatesController(service);

        var templateToCreate = new RoomTemplate
        {
            Name = "New Room",
            Description = "Test",
            Type = RoomType.Combat,
            IsActive = true
        };

        // Act
        var result = await controller.Create(templateToCreate);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var template = Assert.IsType<RoomTemplate>(createdResult.Value);
        Assert.NotEqual(Guid.Empty, template.Id);
        Assert.Equal(templateToCreate.Name, template.Name);
        Assert.Equal("GetById", createdResult.ActionName);
    }

    /// Test: Update retourne Ok avec le template mis à jour
    [Fact]
    public async Task Update_ReturnsOk_WhenUpdateSucceeds()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);
        var controller = new RoomTemplatesController(service);

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
        var result = await controller.Update(original.Id, updated);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var template = Assert.IsType<RoomTemplate>(okResult.Value);
        Assert.Equal("Updated", template.Name);
        Assert.Equal("Updated Description", template.Description);
    }

    /// Test: Delete retourne NoContent quand la suppression réussit
    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleteSucceeds()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);
        var controller = new RoomTemplatesController(service);

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
        var result = await controller.Delete(template.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    /// Test: GetById retourne NotFound quand le template n'existe pas
    [Fact]
    public async Task GetById_ReturnsNotFound_WhenTemplateNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);
        var controller = new RoomTemplatesController(service);

        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await controller.GetById(nonExistentId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    /// Test: Update retourne NotFound quand le template n'existe pas
    [Fact]
    public async Task Update_ReturnsNotFound_WhenTemplateNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);
        var controller = new RoomTemplatesController(service);

        var nonExistentId = Guid.NewGuid();
        var template = new RoomTemplate
        {
            Name = "Updated",
            Description = "Test",
            Type = RoomType.Combat,
            IsActive = true
        };

        // Act
        var result = await controller.Update(nonExistentId, template);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    /// Test: Delete retourne NotFound quand le template n'existe pas
    [Fact]
    public async Task Delete_ReturnsNotFound_WhenTemplateNotExists()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);
        var controller = new RoomTemplatesController(service);

        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await controller.Delete(nonExistentId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    /// Test: GetAll retourne une liste vide quand aucun template actif n'existe
    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoActiveTemplates()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var service = new RoomTemplateService(context);
        var controller = new RoomTemplatesController(service);

        var inactiveTemplate = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Room",
            Description = "Test",
            Type = RoomType.Search,
            IsActive = false
        };

        context.RoomTemplates.Add(inactiveTemplate);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var templates = Assert.IsType<List<RoomTemplate>>(okResult.Value);
        Assert.Empty(templates);
    }
}

