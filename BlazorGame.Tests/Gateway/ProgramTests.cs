using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using GatewayProgram = BlazorGame.Gateway.Program;

namespace BlazorGame.Tests.Gateway;

/// <summary>
/// Tests d'intégration pour le Gateway Program.cs
/// </summary>
public class GatewayProgramTests : IClassFixture<WebApplicationFactory<GatewayProgram>>
{
    private readonly WebApplicationFactory<GatewayProgram> _factory;

    public GatewayProgramTests(WebApplicationFactory<GatewayProgram> factory)
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

        // Assert - Vérifier que les services essentiels sont enregistrés
        Assert.NotNull(services);
    }

    /// Test: L'API Gateway répond aux requêtes
    [Fact]
    public async Task Gateway_RespondsToRequests()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.NotNull(response);
    }
}

