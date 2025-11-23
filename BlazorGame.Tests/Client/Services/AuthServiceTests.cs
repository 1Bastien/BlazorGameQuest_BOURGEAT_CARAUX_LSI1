using System.Net;
using System.Net.Http.Json;
using BlazorGame.Client.Services;
using Blazored.LocalStorage;
using Moq;

namespace BlazorGame.Tests.Client.Services;

/// <summary>
/// Tests unitaires pour le service AuthService
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly Mock<GameService> _mockGameService;

    public AuthServiceTests()
    {
        _mockLocalStorage = new Mock<ILocalStorageService>();
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost") };
        _mockGameService = new Mock<GameService>(httpClient);
    }

    /// Crée un HttpClient avec un handler de test personnalisé
    private HttpClient CreateTestHttpClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost")
        };
    }

    /// Test: LoginAsync retourne true et stocke les tokens quand la connexion réussit
    [Fact]
    public async Task LoginAsync_ReturnsTrue_WhenLoginSuccessful()
    {
        // Arrange
        var tokenResponse = new TokenResponse
        {
            access_token = "test_access_token",
            refresh_token = "test_refresh_token",
            expires_in = 3600,
            token_type = "Bearer"
        };

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/auth/login", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(tokenResponse)
            });
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new AuthService(httpClient, _mockLocalStorage.Object, _mockGameService.Object);

        // Act
        var result = await service.LoginAsync("testuser", "password123");

        // Assert
        Assert.True(result.Success);
        Assert.False(result.IsAccountDisabled);
        _mockLocalStorage.Verify(x => x.SetItemAsStringAsync("access_token", tokenResponse.access_token, default),
            Times.Once);
        _mockLocalStorage.Verify(x => x.SetItemAsStringAsync("refresh_token", tokenResponse.refresh_token, default),
            Times.Once);
    }

    /// Test: LoginAsync retourne false quand les credentials sont invalides
    [Fact]
    public async Task LoginAsync_ReturnsFalse_WhenCredentialsInvalid()
    {
        // Arrange
        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = JsonContent.Create(new { message = "Invalid credentials" })
            });
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new AuthService(httpClient, _mockLocalStorage.Object, _mockGameService.Object);

        // Act
        var result = await service.LoginAsync("wronguser", "wrongpassword");

        // Assert
        Assert.False(result.Success);
        Assert.False(result.IsAccountDisabled);
        _mockLocalStorage.Verify(x => x.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>(), default),
            Times.Never);
    }

    /// Test: LogoutAsync supprime les tokens du LocalStorage
    [Fact]
    public async Task LogoutAsync_RemovesTokens_FromLocalStorage()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItemAsStringAsync("refresh_token", default))
            .ReturnsAsync("test_refresh_token");

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/auth/logout", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new AuthService(httpClient, _mockLocalStorage.Object, _mockGameService.Object);

        // Act
        await service.LogoutAsync();

        // Assert
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("access_token", default), Times.Once);
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("refresh_token", default), Times.Once);
    }

    /// Test: IsAuthenticatedAsync retourne false quand il n'y a pas de token
    [Fact]
    public async Task IsAuthenticatedAsync_ReturnsFalse_WhenNoToken()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItemAsStringAsync("access_token", default))
            .ReturnsAsync((string?)null);

        var httpClient = CreateTestHttpClient(new TestHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))));
        var service = new AuthService(httpClient, _mockLocalStorage.Object, _mockGameService.Object);

        // Act
        var result = await service.IsAuthenticatedAsync();

        // Assert
        Assert.False(result);
    }

    /// Test: RefreshTokenAsync retourne true et met à jour les tokens quand le refresh réussit
    [Fact]
    public async Task RefreshTokenAsync_ReturnsTrue_WhenRefreshSuccessful()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItemAsStringAsync("refresh_token", default))
            .ReturnsAsync("test_refresh_token");

        var newTokenResponse = new TokenResponse
        {
            access_token = "new_access_token",
            refresh_token = "new_refresh_token",
            expires_in = 3600,
            token_type = "Bearer"
        };

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("/api/auth/refresh", request.RequestUri?.PathAndQuery);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(newTokenResponse)
            });
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new AuthService(httpClient, _mockLocalStorage.Object, _mockGameService.Object);

        // Act
        var result = await service.RefreshTokenAsync();

        // Assert
        Assert.True(result);
        _mockLocalStorage.Verify(x => x.SetItemAsStringAsync("access_token", newTokenResponse.access_token, default),
            Times.Once);
        _mockLocalStorage.Verify(x => x.SetItemAsStringAsync("refresh_token", newTokenResponse.refresh_token, default),
            Times.Once);
    }

    /// Test: RefreshTokenAsync retourne false quand le refresh token est invalide
    [Fact]
    public async Task RefreshTokenAsync_ReturnsFalse_WhenRefreshTokenInvalid()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItemAsStringAsync("refresh_token", default))
            .ReturnsAsync("invalid_refresh_token");

        var handler = new TestHttpMessageHandler((request, cancellationToken) =>
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
        });

        var httpClient = CreateTestHttpClient(handler);
        var service = new AuthService(httpClient, _mockLocalStorage.Object, _mockGameService.Object);

        // Act
        var result = await service.RefreshTokenAsync();

        // Assert
        Assert.False(result);
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("access_token", default), Times.Once);
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("refresh_token", default), Times.Once);
    }

    /// Test: GetUserIdAsync retourne le GUID depuis le token
    [Fact]
    public async Task GetUserIdAsync_ReturnsUserId_WhenTokenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token =
            $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{{\"sub\":\"{userId}\",\"exp\":{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()}}}"))}.signature";

        _mockLocalStorage.Setup(x => x.GetItemAsStringAsync("access_token", default))
            .ReturnsAsync(token);

        var httpClient = CreateTestHttpClient(new TestHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))));
        var service = new AuthService(httpClient, _mockLocalStorage.Object, _mockGameService.Object);

        // Act
        var result = await service.GetUserIdAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Value);
    }

    /// Test: GetUserIdAsync retourne null quand il n'y a pas de token
    [Fact]
    public async Task GetUserIdAsync_ReturnsNull_WhenNoToken()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItemAsStringAsync("access_token", default))
            .ReturnsAsync((string?)null);

        var httpClient = CreateTestHttpClient(new TestHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))));
        var service = new AuthService(httpClient, _mockLocalStorage.Object, _mockGameService.Object);

        // Act
        var result = await service.GetUserIdAsync();

        // Assert
        Assert.Null(result);
    }

    /// Test: IsAdminAsync retourne true quand l'utilisateur est admin
    [Fact]
    public async Task IsAdminAsync_ReturnsTrue_WhenUserIsAdmin()
    {
        // Arrange
        var realmAccess = "{\"roles\":[\"administrateur\",\"user\"]}";
        var token =
            $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{{\"realm_access\":{realmAccess},\"exp\":{DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()}}}"))}.signature";

        _mockLocalStorage.Setup(x => x.GetItemAsStringAsync("access_token", default))
            .ReturnsAsync(token);

        var httpClient = CreateTestHttpClient(new TestHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))));
        var service = new AuthService(httpClient, _mockLocalStorage.Object, _mockGameService.Object);

        // Act
        var result = await service.IsAdminAsync();

        // Assert
        Assert.True(result);
    }

    /// Test: IsAdminAsync retourne false quand l'utilisateur n'est pas admin
    [Fact]
    public async Task IsAdminAsync_ReturnsFalse_WhenUserIsNotAdmin()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItemAsStringAsync("access_token", default))
            .ReturnsAsync((string?)null);

        var httpClient = CreateTestHttpClient(new TestHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))));
        var service = new AuthService(httpClient, _mockLocalStorage.Object, _mockGameService.Object);

        // Act
        var result = await service.IsAdminAsync();

        // Assert
        Assert.False(result);
    }
}

