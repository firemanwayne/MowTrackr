using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MowTrackr.Client;
using MowTrackr.Client.Interfaces;
using Radzen;

const string ApiBaseUrlKey = "ApiBaseUrl";

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");

builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration[ApiBaseUrlKey] ?? throw new InvalidOperationException($"Configuration value '{ApiBaseUrlKey}' is missing from wwwroot/appsettings.json.");

builder.Services.AddClientServices(new Uri(apiBaseUrl));

builder.Services.AddRadzenComponents();

var host = builder.Build();

// Apply the saved language before anything renders; it is read back from the profile after sign-in.
await host.Services.GetRequiredService<IPreferencesService>().InitializeAsync();

await host.RunAsync();