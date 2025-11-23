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
                                claimsIdentity.AddClaim(
                                    new System.Security.Claims.Claim("role", role.GetString() ?? ""));
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
builder.Services.Configure<ForwardedHeadersOptions>(options => { options.ForwardedHeaders = ForwardedHeaders.All; });

// Désactiver la délégation HTTP.sys
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

// Ajout des services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BlazorGame Gateway API",
        Version = "v1",
        Description = @"Gateway API pour BlazorGame - Point d'entrée principal de l'application.
        
Pour tester avec authentification :
1. Allez sur http://localhost:5001/swagger (Authentication Service)
2. Utilisez POST /auth/login avec username='admin' et password='admin' (ou un autre utilisateur)
3. Copiez le access_token retourné
4. Cliquez sur 'Authorize' ci-dessus et entrez 'Bearer {votre_token}'
5. Vous pouvez maintenant tester les routes protégées"
    });

    // Configuration de la sécurité Bearer Token
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = @"Authentification JWT.
        
**IMPORTANT:** Entrez UNIQUEMENT le token (sans 'Bearer').
Le préfixe 'Bearer' sera ajouté automatiquement.

**Étapes:**
1. Allez sur http://localhost:5001 et utilisez POST /auth/login
2. Copiez la valeur du champ 'access_token'
3. Revenez ici, cliquez sur 'Authorize' et collez UNIQUEMENT le token
4. Le header sera automatiquement: Authorization: Bearer {votre_token}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Création de l'application
var app = builder.Build();

// Configuration du pipeline HTTP
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlazorGame Gateway API v1");
    c.RoutePrefix = "swagger"; // Swagger UI sur /swagger
});

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
    public partial class Program
    {
    }
}