var builder = DistributedApplication.CreateBuilder(args);

// Local SQL Server container standing in for Azure SQL; data survives restarts via a named volume.
var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("mowtrackr-sql-data");

var database = sql.AddDatabase("mowtrackr");

// Set once with: dotnet user-secrets set Parameters:jwt-signing-key "<32+ random chars>" --project src/AppHost
var jwtSigningKey = builder.AddParameter("jwt-signing-key", secret: true);

var api = builder.AddDotnetProject("api", "../Api/MowTrackr.Api.csproj")
    .WithReference(database)
    .WaitFor(database)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey);

// The WebAssembly client reads its API address from wwwroot/appsettings.json (it cannot see host env vars),
// so both projects use the fixed ports from their launchSettings.json (API 7191, client 7072).
builder.AddDotnetProject("client", "../Blazor/MowTrackr.Client/MowTrackr.Client.csproj")
    .WaitFor(api);

builder.Build().Run();