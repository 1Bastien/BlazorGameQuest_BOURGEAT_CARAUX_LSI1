using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using BlazorGame.Core.Data;
using BlazorGame.Core.Services;
using CoreProgram = BlazorGame.Core.Program;

namespace BlazorGame.Tests.Core;

/// <summary>
/// Tests d'intégration pour Program.cs
/// </summary>
public class ProgramTests : IClassFixture<WebApplicationFactory<CoreProgram>>
{
    private readonly WebApplicationFactory<CoreProgram> _factory;

    public ProgramTests(WebApplicationFactory<CoreProgram> factory)
    {
        _factory = factory;
    }

    /// Test: Les services sont correctement enregistrés
    [Fact]
    public void Services_AreRegistered()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        Assert.NotNull(services.GetService<GameDbContext>());
        Assert.NotNull(services.GetService<UserService>());
        Assert.NotNull(services.GetService<GameRewardsService>());
        Assert.NotNull(services.GetService<RoomTemplateService>());
        Assert.NotNull(services.GetService<GameSessionService>());
        Assert.NotNull(services.GetService<GameActionService>());
    }

    /// Test: L'API répond aux requêtes
    [Fact]
    public async Task Api_RespondsToRequests()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/GameRewards");

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
    }
}

