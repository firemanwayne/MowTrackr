using MowTrackr.Api.Extensions;
using MowTrackr.Api.Options;
using MowTrackr.Assets;
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
    .AddIdentityServices()
    .AddExternalProviders(builder.Configuration);

builder.Services.AddOptions<ClientOptions>().BindConfiguration(ClientOptions.SectionName);

var clientUrl = builder.Configuration[$"{ClientOptions.SectionName}:{nameof(ClientOptions.BaseUrl)}"] ?? new ClientOptions().BaseUrl;

builder.Services.AddCors(options =>
{
    options.AddPolicy(ClientCorsPolicy, policy => policy
        .WithOrigins(clientUrl.TrimEnd('/'))
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