using BlazorGame.Core.Data;
using BlazorGame.Core.Services;
using BlazorGame.Core.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// Création de l'application
var builder = WebApplication.CreateBuilder(args);

// Configuration du DbContext (InMemory pour l'instant)
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseInMemoryDatabase("GameDatabase"));

// Enregistrement des services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GameRewardsService>();
builder.Services.AddScoped<RoomTemplateService>();
builder.Services.AddScoped<GameSessionService>();
builder.Services.AddScoped<GameActionService>();

// Configuration CORS pour le frontend Blazor
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Configuration de l'authentification JWT
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

builder.Services.AddAuthorization();

// Ajout des controllers et configuration JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Ajout des endpoints explorer (Swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BlazorGame Core API",
        Version = "v1",
        Description = @"API principale du jeu BlazorGame. 
        
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

// Initialisation de la base de données
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    context.Database.EnsureCreated();
}

// Configuration du pipeline HTTP
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlazorGame Core API v1");
    c.RoutePrefix = "swagger"; // Swagger UI à la racine
});

app.UseCors("AllowBlazor");

app.UseAuthentication();
app.UseAuthorization();

// Middleware de synchronisation automatique des utilisateurs
// Doit être après UseAuthentication pour avoir accès aux claims JWT
app.UseUserSync();

app.MapControllers();

// Lancement de l'application
app.Run();

// Rendre Program accessible pour les tests
namespace BlazorGame.Core
{
    public partial class Program
    {
    }
}
