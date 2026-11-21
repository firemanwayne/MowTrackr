# src

The shipped projects. Each project folder holds one project (`MowTrackr.<Folder>.csproj`), and namespaces mirror the folders. `Abstractions/`, `Domains/` and `Infrastructure/` only group projects; they hold no code themselves, and the projects inside them use their full `MowTrackr.<Name>` as the folder name. Each project's unit tests sit next to it in a sibling `<Project>.UnitTests` folder (for example `Application/` and `Application.UnitTests/`).

| Folder | What it is |
| --- | --- |
| `AppHost/` | .NET Aspire host. Starts the local SQL Server container, the API and the client together. |
| `ServiceDefaults/` | Shared Aspire defaults: telemetry, health checks and resilience. |
| `Api/` | The HTTP API: controllers only, JWT bearer authentication and external sign-in providers. |
| `Client/` | The Blazor WebAssembly UI (Radzen components): pages, components and layout. |
| `Application/` | The client's non-visual services: typed API clients, session and preferences, the token store, the bearer handler and the authentication state provider. |
| `Abstractions/MowTrackr.Core/` | Contracts (request and response models), service and repository interfaces, constants, options and `Result<T>`. |
| `Domains/MowTrackr.Assets/` | Domain: mowers, manufacturers and their models. |
| `Domains/MowTrackr.Maintenance/` | Domain: service templates, interval validation, due checks and work orders. |
| `Infrastructure/MowTrackr.Identity/` | Users, roles, invitations, password rules and JWT issuing. |
| `Infrastructure/MowTrackr.Communications/` | Sending email (Azure Communication Services, or the log when running locally) and email templates. |
| `Infrastructure/MowTrackr.Data/` | EF Core context, entity configurations, migrations and repositories. |
| `Infrastructure/MowTrackr.Localization/` | UI strings (resx). The English text is the key; `SharedResource.es.resx` holds the Spanish. |

## Tests

Unit tests use xUnit. Each test project is `MowTrackr.<Project>.UnitTests`, sits next to the project it tests, and its
folders mirror that project, so a test for `src/Application/Services/Foo.cs` lives in
`src/Application.UnitTests/Services/FooTests.cs`.

| Test project | Covers |
| --- | --- |
| `Application.UnitTests/` | The client's handlers, providers and services, such as the label text parser. Uses fakes from `Fakes/`. |
| `Domains/MowTrackr.Assets.UnitTests/` | Asset type rules: renaming, and refusing to delete a type that is in use. Uses NSubstitute. |
| `Client.UnitTests/` | The Blazor client's page base class. |
| `Domains/MowTrackr.Maintenance.UnitTests/` | Maintenance rules, such as which inspection templates apply to a piece of equipment. |
| `Abstractions/MowTrackr.Core.UnitTests/` | The shared constants (roles, routes, password policy). |
| `Infrastructure/MowTrackr.Communications.UnitTests/` | Email templates. |
| `Infrastructure/MowTrackr.Data.UnitTests/` | The audit interceptor that stamps who last changed a row (`UpdatedBy`). Uses the EF Core in-memory provider. |
| `Infrastructure/MowTrackr.Identity.UnitTests/` | Identity services and models. Uses the EF Core in-memory provider, NSubstitute and a fake clock (`TimeProvider.Testing`). |

```bash
dotnet test MowTrackr.slnx --configuration Release                 # everything, as CI does
dotnet test src/Application.UnitTests                              # one project
dotnet test src/Application.UnitTests --filter "FullyQualifiedName~LabelTextParser"
```

- Test names follow `MethodName_Scenario_ExpectedResult`, and helper methods go at the bottom of the test class.
- Internal types are reached through `InternalsVisibleTo`, not by making production types public.

## Conventions

- A domain project registers its own services in a `ServiceRegistryExtensions.cs` at its root, and the API calls it.
- Controllers return `IActionResult`; services return `Result<T>`, which a single extension turns into an HTTP result.
- Each project has a `GlobalUsings.cs` for its shared `using` directives.
- Migrations are applied automatically when the API starts. To add one:

  ```bash
  dotnet ef migrations add <Name> --project src/Infrastructure/MowTrackr.Data --startup-project src/Api --output-dir Migrations
  ```

To run everything locally, see the [root README](../README.md#run-locally).
