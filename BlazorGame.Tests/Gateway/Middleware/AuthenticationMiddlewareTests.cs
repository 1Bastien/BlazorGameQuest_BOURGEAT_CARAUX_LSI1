using System.Net;
using BlazorGame.Gateway.Middleware;
using Microsoft.AspNetCore.Http;
using Moq;

namespace BlazorGame.Tests.Gateway.Middleware;

/// <summary>
/// Tests unitaires pour le middleware AuthenticationMiddleware
/// </summary>
public class AuthenticationMiddlewareTests
{
    /// Test: Les routes publiques passent sans authentification
    [Theory]
    [InlineData("/")]
    [InlineData("/index.html")]
    [InlineData("/css/style.css")]
    [InlineData("/js/app.js")]
    [InlineData("/media/logo.png")]
    [InlineData("/favicon.ico")]
    [InlineData("/api/auth/login")]
    public async Task InvokeAsync_AllowsPublicPaths_WithoutAuthentication(string path)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthenticationMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.NotEqual(401, context.Response.StatusCode);
    }

    /// Test: Les routes API avec token valide passent
    [Theory]
    [InlineData("/api/test")]
    [InlineData("/api/other")]
    public async Task InvokeAsync_AllowsApiRoutes_WithValidToken(string path)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Headers["Authorization"] = "Bearer valid_token_here";

        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthenticationMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.NotEqual(401, context.Response.StatusCode);
    }

    /// Test: Les routes d'authentification passent sans token
    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/logout")]
    [InlineData("/api/auth/refresh")]
    public async Task InvokeAsync_AllowsAuthRoutes_WithoutToken(string path)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        var nextCalled = false;
        RequestDelegate next = (HttpContext ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new AuthenticationMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.NotEqual(401, context.Response.StatusCode);
    }
}

