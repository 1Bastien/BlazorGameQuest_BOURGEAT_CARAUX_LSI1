using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorGame.Client;

// Création de l'application
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Ajout des composants à la racine
builder.RootComponents.Add<App>("#app");

// Ajout du head outlet qui sert à injecter la balise head dans le html
builder.RootComponents.Add<HeadOutlet>("head::after");

// Ajout des services
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Lancement de l'application
await builder.Build().RunAsync();
