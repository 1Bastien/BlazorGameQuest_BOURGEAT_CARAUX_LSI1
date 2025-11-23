using System.Net;
using System.Text.Json;
using AuthenticationServices.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace BlazorGame.Tests.AuthenticationServices.Controllers;

/// <summary>
/// Tests unitaires pour le controller AuthController
/// </summary>
public class AuthControllerTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

    public AuthControllerTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        // Configuration par défaut
        _mockConfiguration.Setup(x => x["Keycloak:Authority"]).Returns("http://localhost:8080/realms/blazorgame");
        _mockConfiguration.Setup(x => x["Keycloak:ClientId"]).Returns("blazorgame-client");
    }

    /// Crée un HttpClient avec le mock handler
    private HttpClient CreateMockHttpClient()
    {
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
        return httpClient;
    }

    /// Test: Login retourne OK avec un token quand les credentials sont valides
    [Fact]
    public async Task Login_ReturnsOk_WhenCredentialsValid()
    {
        // Arrange
        var tokenResponse = new TokenResponse
        {
            access_token = "test_token",
            refresh_token = "test_refresh",
            expires_in = 3600,
            token_type = "Bearer"
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
            });

        CreateMockHttpClient();
        var controller = new AuthController(_mockConfiguration.Object, _mockHttpClientFactory.Object);
        var request = new LoginRequest("testuser", "password123");

        // Act
        var result = await controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TokenResponse>(okResult.Value);
        Assert.Equal(tokenResponse.access_token, response.access_token);
    }

    /// Test: Login retourne Unauthorized quand les credentials sont invalides
    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenCredentialsInvalid()
    {
        // Arrange
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            });

        CreateMockHttpClient();
        var controller = new AuthController(_mockConfiguration.Object, _mockHttpClientFactory.Object);
        var request = new LoginRequest("wronguser", "wrongpassword");

        // Act
        var result = await controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    /// Test: Logout retourne OK après déconnexion
    [Fact]
    public async Task Logout_ReturnsOk_WhenLogoutSuccessful()
    {
        // Arrange
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        CreateMockHttpClient();
        var controller = new AuthController(_mockConfiguration.Object, _mockHttpClientFactory.Object);
        var request = new LogoutRequest("test_refresh_token");

        // Act
        var result = await controller.Logout(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    /// Test: RefreshToken retourne OK avec un nouveau token quand le refresh token est valide
    [Fact]
    public async Task RefreshToken_ReturnsOk_WhenRefreshTokenValid()
    {
        // Arrange
        var tokenResponse = new TokenResponse
        {
            access_token = "new_access_token",
            refresh_token = "new_refresh_token",
            expires_in = 3600,
            token_type = "Bearer"
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
            });

        CreateMockHttpClient();
        var controller = new AuthController(_mockConfiguration.Object, _mockHttpClientFactory.Object);
        var request = new RefreshRequest("valid_refresh_token");

        // Act
        var result = await controller.RefreshToken(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<TokenResponse>(okResult.Value);
        Assert.Equal(tokenResponse.access_token, response.access_token);
    }

    /// Test: RefreshToken retourne Unauthorized quand le refresh token est invalide
    [Fact]
    public async Task RefreshToken_ReturnsUnauthorized_WhenRefreshTokenInvalid()
    {
        // Arrange
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            });

        CreateMockHttpClient();
        var controller = new AuthController(_mockConfiguration.Object, _mockHttpClientFactory.Object);
        var request = new RefreshRequest("invalid_refresh_token");

        // Act
        var result = await controller.RefreshToken(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    /// Test: GetUserInfo retourne OK avec les informations utilisateur
    [Fact]
    public async Task GetUserInfo_ReturnsOk_WhenTokenValid()
    {
        // Arrange
        var userInfo = new { sub = Guid.NewGuid().ToString(), preferred_username = "testuser" };
        
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(userInfo))
            });

        CreateMockHttpClient();
        var controller = new AuthController(_mockConfiguration.Object, _mockHttpClientFactory.Object);
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };
        controller.HttpContext.Request.Headers["Authorization"] = "Bearer valid_token";

        // Act
        var result = await controller.GetUserInfo();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    /// Test: GetUserInfo retourne Unauthorized quand le token est manquant
    [Fact]
    public async Task GetUserInfo_ReturnsUnauthorized_WhenTokenMissing()
    {
        // Arrange
        CreateMockHttpClient();
        var controller = new AuthController(_mockConfiguration.Object, _mockHttpClientFactory.Object);
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
        };

        // Act
        var result = await controller.GetUserInfo();

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }

    /// Test: GetUserById retourne OK avec les informations utilisateur
    [Fact]
    public async Task GetUserById_ReturnsOk_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenResponse = new TokenResponse { access_token = "admin_token" };
        var userInfo = new { username = "testuser", realmRoles = new[] { "user" } };

        var callCount = 0;
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
                    };
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(userInfo))
                };
            });

        _mockConfiguration.Setup(x => x["Keycloak:AdminUsername"]).Returns("admin");
        _mockConfiguration.Setup(x => x["Keycloak:AdminPassword"]).Returns("admin123");

        CreateMockHttpClient();
        var controller = new AuthController(_mockConfiguration.Object, _mockHttpClientFactory.Object);

        // Act
        var result = await controller.GetUserById(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    /// Test: GetUserById retourne NotFound quand l'utilisateur n'existe pas
    [Fact]
    public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenResponse = new TokenResponse { access_token = "admin_token" };

        var callCount = 0;
        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
                    };
                }
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
            });

        _mockConfiguration.Setup(x => x["Keycloak:AdminUsername"]).Returns("admin");
        _mockConfiguration.Setup(x => x["Keycloak:AdminPassword"]).Returns("admin123");

        CreateMockHttpClient();
        var controller = new AuthController(_mockConfiguration.Object, _mockHttpClientFactory.Object);

        // Act
        var result = await controller.GetUserById(userId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }
}

