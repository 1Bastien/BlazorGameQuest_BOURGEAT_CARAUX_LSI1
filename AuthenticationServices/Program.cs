// Création de l'application
var builder = WebApplication.CreateBuilder(args);

// Ajout des services au conteneur
builder.Services.AddControllers();

// Ajout des endpoints explorer
builder.Services.AddEndpointsApiExplorer();

// Création de l'application    
var app = builder.Build();

// Utilisation de l'autorisation
app.UseAuthorization();

// Mapping des controllers
app.MapControllers();

// Lancement de l'application
app.Run();