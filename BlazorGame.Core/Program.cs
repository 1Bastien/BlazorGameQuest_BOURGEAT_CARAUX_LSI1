using BlazorGame.Core.Data;
using BlazorGame.Core.Services;
using Microsoft.EntityFrameworkCore;


// Création de l'application
var builder = WebApplication.CreateBuilder(args);

// Configuration du DbContext (InMemory pour l'instant)
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseInMemoryDatabase("GameDatabase"));

// Enregistrement des services
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

// Ajout des controllers et configuration JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Ajout des endpoints explorer (Swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Création de l'application    
var app = builder.Build();

// Initialisation de la base de données
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    context.Database.EnsureCreated();
}

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowBlazor");

app.UseAuthorization();

app.MapControllers();

// Lancement de l'application
app.Run();
