# MowTrackr

Asset management and maintenance tracking for lawn mowers. A manager registers mowers, defines service templates
(checklists with an hours and/or days interval), sees what is due, and schedules work orders that technicians check off.

## Stack

- **Client** - Blazor WebAssembly with [Radzen Blazor](https://blazor.radzen.com) components
- **API** - ASP.NET Core controllers only (no Blazor); JWT bearer authentication
- **Data** - Azure SQL via EF Core (SQL Server container locally)
- **Orchestration** - .NET Aspire AppHost (local SQL container, API and client with telemetry)
- **Sign-in** - local accounts plus optional Google, Facebook and LinkedIn
- **CI/CD** - GitHub Actions; deploys to Azure free tiers (see [docs/azure-setup.md](docs/azure-setup.md))

## Layout

```
src/
  AppHost/          Aspire host - runs SQL, API and client locally
  ServiceDefaults/  Shared Aspire defaults (telemetry, health checks, resilience)
  Api/              HTTP API (controllers, JWT, external providers)
  Client/           Blazor WebAssembly UI
  Core/             Contracts (request/response models), interfaces, constants, Result<T>
  Assets/           Domain: mowers
  Maintenance/      Domain: service templates, interval validation, due checks, work orders
  Identity/         Domain: users, roles, JWT issuing
  Data/             EF Core context, migrations, repositories
  Localization/     UI strings (resx): English text is the key, SharedResource.es.resx holds Spanish
```

## Run locally

Prerequisites: .NET 10 SDK and Docker (for the SQL Server container).

```bash
# one-time: a signing key for local JWTs
dotnet user-secrets set "Parameters:jwt-signing-key" "<32+ random characters>" --project src/AppHost

dotnet run --project src/AppHost
```

The Aspire dashboard shows the SQL container, API (`https://localhost:7191`) and client (`https://localhost:7072`).
The first account you register becomes **Admin**; later accounts start as **Technician** (an Admin can change roles).

### External sign-in (optional)

Add credentials to the API's user secrets to enable a provider, e.g.:

```bash
dotnet user-secrets set "Authentication:Google:ClientId" "..." --project src/Api
dotnet user-secrets set "Authentication:Google:ClientSecret" "..." --project src/Api
```

A provider with no credentials is simply hidden on the sign-in page.

## Language and theme

Each user picks a **language** (English / Spanish) and an **appearance** (Light / Dark) on their profile page. The choice is
stored on the profile and re-applied at sign-in; a copy is cached in the browser so the sign-in page already uses it.
Changing the language reloads the page so every string switches. To add a string, wrap it as `L["English text"]` and add the
Spanish entry to `src/Localization/Resources/SharedResource.es.resx`. API validation/error messages are still English.

## Maintenance intervals

A service template is due every N engine hours, every N days, or both (whichever comes first), measured from the last
completed work order for that mower (or from purchase / zero hours if it has never been serviced). When you define an
interval the app checks it against your other active templates: an exact duplicate is an **error** (blocks saving), and an
interval that is a multiple of another (for example 100 h vs 50 h, so both are due together) is a **warning**.
The "coming due" window is configurable under `Maintenance:LookaheadHours` / `Maintenance:LookaheadDays`.

## Migrations

```bash
dotnet ef migrations add <Name> --project src/Data --startup-project src/Api --output-dir Migrations
```

Migrations are applied automatically when the API starts.