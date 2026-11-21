# Azure setup

MowTrackr runs in two environments, `dev` and `prod`, each created and updated by Bicep (`infra/`) through GitHub
Actions. The layout follows the AlertOne repo (per-environment parameter files, `<app>-<resource>-<env>-<instance>` names).

| Piece | Azure resource | dev | prod |
| --- | --- | --- | --- |
| Client | Static Web App | Free | Free |
| API | App Service (Linux, .NET 10), system-assigned managed identity | F1 Free | F1 Free |
| Database | Azure SQL Database, Microsoft Entra-only authentication | [Free offer](https://learn.microsoft.com/azure/azure-sql/database/free-offer) | serverless GP_S_Gen5_1 (paid, auto-pauses) |
| Secrets | Key Vault (RBAC) | yes | yes |
| Email (invitations) | Azure Communication Services + Email Service, Azure-managed sender domain | yes | yes |

> Only one database per subscription can use the free SQL offer, so dev takes it and prod runs a small paid
> serverless database. The databases auto-pause when idle, so the first request after a quiet period can take a few
> seconds; the API retries transient connection failures automatically. The free App Service plan has no always-on
> and a daily CPU quota; set `appServicePlanSku = 'B1'` in `infra/parameters/prod.bicepparam` when prod needs more.

## How it fits together

```
infra/
  main.bicep                  One environment: wires the modules, app settings and the Key Vault access
  modules/sql.bicep           SQL server (Entra-only) and database
  modules/key-vault.bicep     Key Vault and its secrets
  modules/communication.bicep Communication Services, Email Service and an Azure-managed sender domain
  modules/api.bicep           Plan and web app with a managed identity
  modules/staticwebapp.bicep  Static Web App
  parameters/dev.bicepparam   Per-environment values (names, SKUs, free-offer switch, secrets from env variables)
  parameters/prod.bicepparam
  bootstrap.ps1               One-time setup of resource groups, GitHub sign-in and environments
.github/workflows/
  ci.yml                      Build and Bicep validation on every push and pull request
  deploy.yml                  Build, deploy infra/, publish the API and the client
```

- **No passwords.** The API's managed identity is the SQL Entra administrator, so it runs EF Core migrations and
  queries with `Authentication=Active Directory Managed Identity`. Nobody else can log in to the database until you
  change the SQL admin in the portal (there is one Entra admin per server).
- **Invitation email.** Mail is sent from `invitations@<domain>`: `invitations@mowtrackr.com` in prod and
  `invitations@mail.dev.mowtrackr.com` in dev (dev uses a `mail.` subdomain because `dev.mowtrackr.com` is a CNAME to the
  client, and a CNAME cannot share a name with TXT records). The domain is authenticated with SPF, DKIM and a DMARC
  policy, so it is trusted by mail providers; mail from the shared Azure-managed domain (`xxxx.azurecomm.net`) tends to
  land in spam. The deploy creates the domain and its DNS records in the shared zone, then
  `infra/scripts/email-domain.sh` asks Azure to verify them (DNS can take a few minutes) and a second pass links the
  domain. Until that finishes, or if it ever fails, mail still goes out from `invitations@` on the Azure-managed domain.
  The API gets `Communications__SenderAddress` plus the connection string (kept in Key Vault).
- **Secrets in Key Vault.** The JWT signing key, OAuth client secrets and the Communication Services connection string are
  stored in the vault and read by the API
  through Key Vault references in its app settings.
- **No stored Azure credentials.** GitHub signs in with OpenID Connect. The app registration has Contributor and
  Role Based Access Control Administrator on the two MowTrackr resource groups only.

## One-time setup

Prerequisites: the Azure CLI (`az login`) and GitHub CLI (`gh auth login`), signed in, with permission to create app
registrations and role assignments.

```powershell
./infra/bootstrap.ps1 -Repository firemanwayne/MowTrackr
```

It creates `mowtrackr-rg-dev` and `mowtrackr-rg-prod`, the OIDC app registration with a federated credential per
environment, the GitHub `dev` and `prod` environments, and sets:

| Scope | Kind | Name | Value |
| --- | --- | --- | --- |
| Repository | Variable | `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` | the OIDC app and subscription |
| Environment | Variable | `AZURE_RESOURCE_GROUP` | `mowtrackr-rg-<env>` |
| Environment | Secret | `JWT_SIGNING_KEY` | generated (48 characters), never rotated by a re-run |

Add required reviewers to `prod` (Settings > Environments) if you want an approval step.

### External sign-in (optional)

Add any of these as **environment secrets** and the next deploy enables the provider; a provider with no credentials is
hidden on the sign-in page.

`GOOGLE_CLIENT_ID` / `GOOGLE_CLIENT_SECRET`, `FACEBOOK_CLIENT_ID` / `FACEBOOK_CLIENT_SECRET`,
`LINKEDIN_CLIENT_ID` / `LINKEDIN_CLIENT_SECRET`

Register each provider's OAuth redirect URI as `https://<your-api>.azurewebsites.net/signin-<provider>`
(`signin-google`, `signin-facebook`, `signin-linkedin`). The API URL is shown in the Deploy run summary.

## Deploying

Branches: work happens on feature branches, which merge into `dev` by pull request. `dev` holds what is deployed to
the dev site. When dev looks right, open a pull request from `dev` into `main`. Both branches are protected: changes
arrive by pull request only, and the `build` check must pass.

- **Push (merge) to `dev`** deploys **dev**.
- **Run Deploy from the Actions tab** to pick `dev` or `prod`. `prod` can only be run from `main`. Tick *what-if* to
  preview the infrastructure changes without deploying anything.
- Merging to `main` does not deploy anything by itself.

Each run: builds and validates, deploys `infra/main.bicep` with that environment's parameter file (idempotent), then
publishes the API to App Service and the client to the Static Web App with the client pointed at the API URL. The API
applies EF Core migrations on startup, so the first request after a deploy creates or updates the schema.

Until the `AZURE_*` variables exist the deploy jobs skip themselves; CI always runs.

## Changing things

- **Name taken?** App Service and Key Vault names are global. Change `instance` in the parameter file (for example `002`).
- **Different region:** override `location` and `staticWebAppLocation` (the Static Web App supports only a few regions).
- **Tear down an environment:** delete its resource group. Key Vaults are soft-deleted, so purge `mowtrackr-kv-<env>-001`
  before redeploying under the same name.
