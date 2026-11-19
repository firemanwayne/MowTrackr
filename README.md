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
  Identity/         Domain: users, roles, invitations, JWT issuing
  Communications/   Email sending (Azure Communication Services, or the log locally)
  Data/             EF Core context, migrations, repositories
  Localization/     UI strings (resx): English text is the key, SharedResource.es.resx holds Spanish
tests/              Unit tests (Identity, Communications): dotnet test
```

## Run locally

Prerequisites: .NET 10 SDK and Docker (for the SQL Server container).

```bash
# one-time: a signing key for local JWTs
dotnet user-secrets set "Parameters:jwt-signing-key" "<32+ random characters>" --project src/AppHost

dotnet run --project src/AppHost
```

The Aspire dashboard shows the SQL container, API (`https://localhost:7191`) and client (`https://localhost:7072`).
The first account you register becomes **Admin** and must confirm its email before it can sign in; locally nothing is sent, so the confirmation link is written to the API log (the Aspire dashboard) - copy it into the browser. From then on MowTrackr is **invitation only** (see below).

Passwords need 12 to 128 characters with upper case, lower case, a digit and a symbol, and must not contain your name or email or be a common password. Phone numbers are saved only after a texted code is entered (locally the code is also written to the log). See [docs/security.md](docs/security.md) for the security review.

## Invitations

Open registration closes as soon as the first account exists. After that an **Admin** invites people from the
**Invitations** page: pick an email address and a role (Admin, Manager or Technician), and MowTrackr emails a link. The
invitee opens it, chooses a name and password, and is signed in with the invited role. Admins can **resend** (a new link
and a fresh 7 days; the old link stops working) or **revoke** an invitation at any time. Links are single-use, expire
after 7 days, and only a hash of the secret is stored. People who sign in with Google, Facebook or LinkedIn also need an
invitation for their email.

Email goes out through Azure Communication Services. Locally, with no `Communications` settings, nothing is sent and the
invitation link is written to the API's log (the Aspire dashboard), so you can copy it into the browser. To send real
mail locally:

```bash
dotnet user-secrets set "Communications:ConnectionString" "<Communication Services connection string>" --project src/Api
dotnet user-secrets set "Communications:SenderAddress" "DoNotReply@<your-domain>.azurecomm.net" --project src/Api
```

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