namespace BlazorGame.Gateway.Middleware;

/// <summary>
/// Middleware pour vérifier l'authentification des requêtes API
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string[] _publicPaths = { "/", "/index.html", "/css", "/js", "/media", "/favicon", "/icon", "/api/auth" };

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Vérifie la présence d'un token JWT pour toutes les routes API sauf les routes publiques
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        // Vérifier si c'est une route publique
        bool isPublicPath = _publicPaths.Any(p => path.StartsWith(p));

        if (!isPublicPath && path.StartsWith("/api"))
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { message = "Authentication required" });
                return;
            }
        }

        await _next(context);
    }
}

