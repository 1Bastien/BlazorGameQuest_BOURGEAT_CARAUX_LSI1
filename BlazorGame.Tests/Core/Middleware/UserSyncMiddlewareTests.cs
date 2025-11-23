using BlazorGame.Core.Data;
using BlazorGame.Core.Middleware;
using BlazorGame.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlazorGame.Tests;

/// <summary>
/// Tests unitaires pour le middleware UserSyncMiddleware
/// </summary>
public class UserSyncMiddlewareTests
{
    /// Crée un contexte de base de données en mémoire pour les tests
    private GameDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GameDbContext(options);
    }

    /// Crée un HttpContext avec un utilisateur authentifié
    private DefaultHttpContext CreateHttpContextWithUser(Guid userId, string username = "testuser", string? role = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username)
        };

        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim("role", role));
        }

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
    }

    /// Test: Le middleware synchronise un utilisateur authentifié
    [Fact]
    public async Task InvokeAsync_SynchronizesUser_WhenAuthenticated()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var userId = Guid.NewGuid();
        var username = "testuser";
        var role = "Player";

        var httpContext = CreateHttpContextWithUser(userId, username, role);
        var nextCalled = false;
        RequestDelegate next = (_) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new UserSyncMiddleware(next);

        // Act
        await middleware.InvokeAsync(httpContext, userService);

        // Assert
        Assert.True(nextCalled);
        var userInDb = await context.Users.FindAsync(userId);
        Assert.NotNull(userInDb);
        Assert.Equal(userId, userInDb.Id);
        Assert.Equal(username, userInDb.Username);
    }

    /// Test: Le middleware ne fait rien si l'utilisateur n'est pas authentifié
    [Fact]
    public async Task InvokeAsync_DoesNothing_WhenUserNotAuthenticated()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var httpContext = new DefaultHttpContext();
        var middleware = new UserSyncMiddleware((_) => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(httpContext, userService);

        // Assert
        Assert.Empty(context.Users);
    }

    /// Test: Le middleware ne fait rien si le claim userId est manquant
    [Fact]
    public async Task InvokeAsync_DoesNothing_WhenUserIdClaimMissing()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        var middleware = new UserSyncMiddleware((_) => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(httpContext, userService);

        // Assert
        Assert.Empty(context.Users);
    }

    /// Test: Le middleware ne fait rien si le userId n'est pas un GUID valide
    [Fact]
    public async Task InvokeAsync_DoesNothing_WhenUserIdIsNotValidGuid()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid-guid"),
            new Claim(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        var middleware = new UserSyncMiddleware((_) => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(httpContext, userService);

        // Assert
        Assert.Empty(context.Users);
    }

    /// Test: Le middleware met à jour le username s'il était vide
    [Fact]
    public async Task InvokeAsync_UpdatesUsername_WhenUserExistsWithEmptyUsername()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var userId = Guid.NewGuid();
        var newUsername = "newusername";

        await userService.EnsureUserExistsAsync(userId);

        var httpContext = CreateHttpContextWithUser(userId, newUsername, "Player");
        var middleware = new UserSyncMiddleware((_) => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(httpContext, userService);

        // Assert
        var userInDb = await context.Users.FindAsync(userId);
        Assert.NotNull(userInDb);
        Assert.Equal(newUsername, userInDb.Username);
    }

    /// Test: Le middleware met à jour le rôle s'il a changé
    [Fact]
    public async Task InvokeAsync_UpdatesRole_WhenRoleChanged()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var userService = new UserService(context);
        var userId = Guid.NewGuid();
        var username = "testuser";
        var oldRole = "Player";
        var newRole = "Admin";

        await userService.EnsureUserExistsAsync(userId, username: username, role: oldRole);

        var httpContext = CreateHttpContextWithUser(userId, username, newRole);
        var middleware = new UserSyncMiddleware((_) => Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(httpContext, userService);

        // Assert
        var userInDb = await context.Users.FindAsync(userId);
        Assert.NotNull(userInDb);
        Assert.Equal(newRole, userInDb.Role);
    }
}
