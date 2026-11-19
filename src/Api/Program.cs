using MowTrackr.Api.Extensions;
using MowTrackr.Assets;
using MowTrackr.Communications;
using MowTrackr.Core.Options;
using MowTrackr.Data;
using MowTrackr.Identity;
using MowTrackr.Maintenance;

const string ClientCorsPolicy = "Client";

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Data must be registered before Identity so migrations run before roles are seeded.
builder.AddDataServices();

builder.Services
    .AddAssetsServices()
    .AddMaintenanceServices()
    .AddCommunicationServices()
    .AddIdentityServices()
    .AddExternalProviders(builder.Configuration);

builder.Services.AddOptions<ClientOptions>().BindConfiguration(ClientOptions.SectionName);

var clientUrl = builder.Configuration[$"{ClientOptions.SectionName}:{nameof(ClientOptions.BaseUrl)}"] ?? new ClientOptions().BaseUrl;

builder.Services.AddCors(options =>
{
    options.AddPolicy(ClientCorsPolicy, policy => policy
        .SetIsOriginAllowed(origin => IsAllowedOrigin(origin, clientUrl, builder.Environment.IsDevelopment()))
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(ClientCorsPolicy);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Decides whether a browser origin may call the API: the configured client, or any localhost origin in Development.
static bool IsAllowedOrigin(string origin, string clientUrl, bool isDevelopment)
{
    if (string.Equals(origin.TrimEnd('/'), clientUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    return isDevelopment && Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.IsLoopback;
}
