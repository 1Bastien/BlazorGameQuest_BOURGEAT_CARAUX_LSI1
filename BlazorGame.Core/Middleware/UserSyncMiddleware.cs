using BlazorGame.Core.Services;
using System.Security.Claims;

namespace BlazorGame.Core.Middleware;

/// <summary>
/// Middleware qui synchronise automatiquement l'utilisateur authentifié avec la base de données
/// </summary>
public class UserSyncMiddleware
{
    private readonly RequestDelegate _next;

    public UserSyncMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserService userService)
    {
        // Vérifier si l'utilisateur est authentifié
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Extraire les informations de l'utilisateur depuis les claims JWT
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? context.User.FindFirst("sub")?.Value;
            
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                var username = context.User.FindFirst(ClaimTypes.Name)?.Value 
                            ?? context.User.FindFirst("preferred_username")?.Value;
                
                var role = context.User.FindFirst("role")?.Value;

                // Synchroniser l'utilisateur avec la base de données
                // Cette opération est idempotente : elle crée l'utilisateur s'il n'existe pas,
                // ou met à jour ses informations si elles ont changé
                await userService.EnsureUserExistsAsync(userId, username, role);
            }
        }

        // Continuer le pipeline
        await _next(context);
    }
}

/// <summary>
/// Extension pour faciliter l'ajout du middleware
/// </summary>
public static class UserSyncMiddlewareExtensions
{
    public static IApplicationBuilder UseUserSync(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserSyncMiddleware>();
    }
}

