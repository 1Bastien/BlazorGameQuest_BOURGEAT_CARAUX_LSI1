using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BlazorGame.Gateway.Middleware;

// Création du builder
var builder = WebApplication.CreateBuilder(args);

// Configuration CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configuration de l'authentification JWT avec Keycloak
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var authority = builder.Configuration["Keycloak:Authority"];
        options.Authority = authority;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = "role"
        };
        
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                if (claimsIdentity != null)
                {
                    // Extraire les rôles de realm_access.roles
                    var realmAccess = context.Principal?.FindFirst("realm_access")?.Value;
                    if (!string.IsNullOrEmpty(realmAccess))
                    {
                        var realmAccessObj = System.Text.Json.JsonDocument.Parse(realmAccess);
                        if (realmAccessObj.RootElement.TryGetProperty("roles", out var rolesElement))
                        {
                            foreach (var role in rolesElement.EnumerateArray())
                            {
                                claimsIdentity.AddClaim(new System.Security.Claims.Claim("role", role.GetString() ?? ""));
                            }
                        }
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

// Configuration de l'autorisation avec les rôles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireJoueurRole", policy => 
        policy.RequireRole("joueur", "administrateur"));
    options.AddPolicy("RequireAdminRole", policy => 
        policy.RequireRole("administrateur"));
});

// Configuration du reverse proxy pour le frontend
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configuration des options HTTP pour le proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
});

// Désactiver la délégation HTTP.sys
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

// Ajout des services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Création de l'application
var app = builder.Build();

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("AllowAll");

// Middleware d'authentification personnalisé
app.UseMiddleware<AuthenticationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Configuration du reverse proxy
app.MapReverseProxy();

// Fallback pour les routes non-API vers le frontend Blazor
app.MapFallbackToFile("index.html");

app.Run();

// Rendre Program accessible pour les tests
namespace BlazorGame.Gateway
{
    public partial class Program { }
}