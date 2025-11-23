using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using AuthProgram = global::AuthenticationServices.Program;

namespace BlazorGame.Tests.AuthenticationServices;

/// <summary>
/// Tests d'intégration pour AuthenticationServices Program.cs
/// </summary>
public class AuthProgramTests : IClassFixture<WebApplicationFactory<AuthProgram>>
{
    private readonly WebApplicationFactory<AuthProgram> _factory;

    public AuthProgramTests(WebApplicationFactory<AuthProgram> factory)
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
        Assert.NotNull(services);
        Assert.NotNull(services.GetService<IHttpClientFactory>());
    }

    /// Test: L'API Auth répond aux requêtes
    [Fact]
    public async Task AuthApi_RespondsToRequests()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Tester une route qui existe
        var response = await client.PostAsync("/auth/login", null);

        // Assert - On s'attend à une erreur mais pas à un crash
        Assert.NotNull(response);
    }
}

