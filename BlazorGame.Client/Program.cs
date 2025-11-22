using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorGame.Client;
using BlazorGame.Client.Services;
using Blazored.LocalStorage;

// Création de l'application
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Ajout des composants à la racine
builder.RootComponents.Add<App>("#app");

// Ajout du head outlet qui sert à injecter la balise head dans le html
builder.RootComponents.Add<HeadOutlet>("head::after");

// Ajout du LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Ajout des services
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<AdminService>();

// Lancement de l'application
var app = builder.Build();

// Initialiser l'authentification
var authService = app.Services.GetRequiredService<AuthService>();
await authService.IsAuthenticatedAsync();

await app.RunAsync();
