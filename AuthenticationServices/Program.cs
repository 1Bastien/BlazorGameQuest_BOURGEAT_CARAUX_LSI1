// Création de l'application

var builder = WebApplication.CreateBuilder(args);

// Ajout des services au conteneur
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Ajout des endpoints explorer
builder.Services.AddEndpointsApiExplorer();

// Configuration de Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Authentication Service API",
        Version = "v1",
        Description =
            "API pour l'authentification via Keycloak. Utilisez la route POST /auth/login pour obtenir un token JWT."
    });

    // Configuration de la sécurité Bearer Token
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = @"Authentification JWT. 
        
**IMPORTANT:** Entrez UNIQUEMENT le token (sans 'Bearer').
Le préfixe 'Bearer' sera ajouté automatiquement.

**Étapes:**
1. Utilisez POST /auth/login pour obtenir un token
2. Copiez la valeur du champ 'access_token'
3. Cliquez sur 'Authorize' et collez UNIQUEMENT le token
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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Authentication Service API v1");
    c.RoutePrefix = "swagger"; // Swagger UI à la racine
});

// Utilisation de l'autorisation
app.UseAuthorization();

// Mapping des controllers
app.MapControllers();

// Lancement de l'application
app.Run();

// Rendre Program accessible pour les tests
namespace AuthenticationServices
{
    public partial class Program
    {
    }
}