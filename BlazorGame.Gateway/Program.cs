// Création du builder
var builder = WebApplication.CreateBuilder(args);

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

// Permet de servir les fichiers statiques
app.UseStaticFiles();

// Utilisation du routing et de l'autorisation
app.UseRouting();
app.UseAuthorization();

// Configuration des endpoints
app.MapControllers();

// Lancement de l'application
app.Run();