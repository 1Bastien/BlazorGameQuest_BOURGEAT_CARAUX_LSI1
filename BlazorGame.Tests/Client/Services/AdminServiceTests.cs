using System.Net;
using System.Net.Http.Json;
using BlazorGame.Client.Services;
using SharedModels.Entities;
using SharedModels.Enums;

namespace BlazorGame.Tests;

/// <summary>
/// Tests unitaires pour le service AdminService
/// </summary>
public class AdminServiceTests
{
    /// Crée un HttpClient avec un handler de test personnalisé
    private HttpClient CreateTestHttpClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };
    }

    /// Test: GetGameRewardsAsync retourne la configuration des récompenses
    [Fact]
    public async Task GetGameRewardsAsync_ReturnsGameRewards_WhenSuccessful()
    {
        // Arrange
        var expectedConfig = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 100,
            MinCombatVictoryPoints = 10,
            MaxCombatVictoryPoints = 20
        };

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/GameRewards", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedConfig)
            });
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new AdminService(httpClient);

        // Act
        var result = await service.GetGameRewardsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedConfig.Id, result.Id);
        Assert.Equal(expectedConfig.StartingHealth, result.StartingHealth);
    }

    /// Test: UpdateGameRewardsAsync met à jour la configuration
    [Fact]
    public async Task UpdateGameRewardsAsync_UpdatesConfig_WhenSuccessful()
    {
        // Arrange
        var configToUpdate = new GameRewards
        {
            Id = Guid.NewGuid(),
            StartingHealth = 150
        };

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal("/api/GameRewards", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(configToUpdate)
            });
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new AdminService(httpClient);

        // Act
        var result = await service.UpdateGameRewardsAsync(configToUpdate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(configToUpdate.Id, result.Id);
        Assert.Equal(configToUpdate.StartingHealth, result.StartingHealth);
    }

    /// Test: GetAllRoomTemplatesAsync retourne tous les templates
    [Fact]
    public async Task GetAllRoomTemplatesAsync_ReturnsAllTemplates_WhenSuccessful()
    {
        // Arrange
        var expectedTemplates = new List<RoomTemplate>
        {
            new RoomTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Room 1",
                Description = "Test 1",
                Type = RoomType.Combat,
                IsActive = true
            },
            new RoomTemplate
            {
                Id = Guid.NewGuid(),
                Name = "Room 2",
                Description = "Test 2",
                Type = RoomType.Search,
                IsActive = false
            }
        };

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/RoomTemplates/admin", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expectedTemplates)
            });
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new AdminService(httpClient);

        // Act
        var result = await service.GetAllRoomTemplatesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    /// Test: CreateRoomTemplateAsync crée un nouveau template
    [Fact]
    public async Task CreateRoomTemplateAsync_CreatesTemplate_WhenSuccessful()
    {
        // Arrange
        var templateToCreate = new RoomTemplate
        {
            Name = "New Room",
            Description = "Test",
            Type = RoomType.Combat,
            IsActive = true
        };

        var createdTemplate = new RoomTemplate
        {
            Id = Guid.NewGuid(),
            Name = templateToCreate.Name,
            Description = templateToCreate.Description,
            Type = templateToCreate.Type,
            IsActive = templateToCreate.IsActive
        };

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/RoomTemplates", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(createdTemplate)
            });
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new AdminService(httpClient);

        // Act
        var result = await service.CreateRoomTemplateAsync(templateToCreate);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(templateToCreate.Name, result.Name);
    }

    /// Test: UpdateRoomTemplateAsync met à jour un template existant
    [Fact]
    public async Task UpdateRoomTemplateAsync_UpdatesTemplate_WhenSuccessful()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var templateToUpdate = new RoomTemplate
        {
            Id = templateId,
            Name = "Updated Room",
            Description = "Updated",
            Type = RoomType.Search,
            IsActive = false
        };

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Put, request.Method);
            Assert.Equal($"/api/RoomTemplates/{templateId}", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(templateToUpdate)
            });
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new AdminService(httpClient);

        // Act
        var result = await service.UpdateRoomTemplateAsync(templateId, templateToUpdate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(templateId, result.Id);
        Assert.Equal(templateToUpdate.Name, result.Name);
    }

    /// Test: DeleteRoomTemplateAsync supprime un template avec succès
    [Fact]
    public async Task DeleteRoomTemplateAsync_ReturnsTrue_WhenSuccessful()
    {
        // Arrange
        var templateId = Guid.NewGuid();

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Delete, request.Method);
            Assert.Equal($"/api/RoomTemplates/{templateId}", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new AdminService(httpClient);

        // Act
        var result = await service.DeleteRoomTemplateAsync(templateId);

        // Assert
        Assert.True(result);
    }
}

/// <summary>
/// Handler de test pour simuler les réponses HTTP
/// </summary>
public class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

    public TestHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
    {
        _sendAsync = sendAsync;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return _sendAsync(request, cancellationToken);
    }
}

