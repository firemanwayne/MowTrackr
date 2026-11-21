# src

The shipped projects. Each folder holds one project (`MowTrackr.<Folder>.csproj`), and namespaces mirror the folders.

| Folder | What it is |
| --- | --- |
| `AppHost/` | .NET Aspire host. Starts the local SQL Server container, the API and the client together. |
| `ServiceDefaults/` | Shared Aspire defaults: telemetry, health checks and resilience. |
| `Api/` | The HTTP API: controllers only, JWT bearer authentication and external sign-in providers. |
| `Client/` | The Blazor WebAssembly UI (Radzen components). Pages, components, layout and API services. |
| `Core/` | Contracts (request and response models), service and repository interfaces, constants, options and `Result<T>`. |
| `Assets/` | Domain: mowers, manufacturers and their models. |
| `Maintenance/` | Domain: service templates, interval validation, due checks and work orders. |
| `Identity/` | Domain: users, roles, invitations, password rules and JWT issuing. |
| `Communications/` | Sending email (Azure Communication Services, or the log when running locally) and email templates. |
| `Data/` | EF Core context, entity configurations, migrations and repositories. |
| `Localization/` | UI strings (resx). The English text is the key; `SharedResource.es.resx` holds the Spanish. |

## Conventions

- A domain project registers its own services in a `ServiceRegistryExtensions.cs` at its root, and the API calls it.
- Controllers return `IActionResult`; services return `Result<T>`, which a single extension turns into an HTTP result.
- Each project has a `GlobalUsings.cs` for its shared `using` directives.
- Migrations are applied automatically when the API starts. To add one:

  ```bash
  dotnet ef migrations add <Name> --project src/Data --startup-project src/Api --output-dir Migrations
  ```

To run everything locally, see the [root README](../README.md#run-locally).
