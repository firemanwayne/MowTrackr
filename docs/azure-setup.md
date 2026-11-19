# Azure setup (free tier)

MowTrackr deploys as three Azure resources, all on free offers:

| Piece | Azure resource | Tier |
| --- | --- | --- |
| Blazor WebAssembly client | Static Web App | Free |
| API | App Service (Linux, .NET 10) | F1 Free |
| Database | Azure SQL Database | [Free offer](https://learn.microsoft.com/azure/azure-sql/database/free-offer) (serverless, auto-pause) |

> The free SQL database auto-pauses when idle, so the first request after a quiet period can take a few
> seconds. The API retries transient connection failures automatically.

## 1. Create the resources

```bash
az group create --name mowtrackr-rg --location eastus2

# SQL server + free-offer database
az sql server create --name <unique-sql-server> --resource-group mowtrackr-rg --location eastus2 \
  --admin-user mowtrackradmin --admin-password '<strong-password>'
az sql db create --resource-group mowtrackr-rg --server <unique-sql-server> --name mowtrackr \
  --edition GeneralPurpose --family Gen5 --capacity 2 --compute-model Serverless --use-free-limit --free-limit-exhaustion-behavior AutoPause
az sql server firewall-rule create --resource-group mowtrackr-rg --server <unique-sql-server> \
  --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# API (App Service, free F1)
az appservice plan create --name mowtrackr-plan --resource-group mowtrackr-rg --sku F1 --is-linux
az webapp create --name <unique-api-name> --resource-group mowtrackr-rg --plan mowtrackr-plan --runtime "DOTNETCORE:10.0"

# Client (Static Web App, free)
az staticwebapp create --name <unique-client-name> --resource-group mowtrackr-rg --sku Free --location eastus2
```

## 2. Configure the API

Set these as App Service application settings (double underscore = `:` in config):

| Setting | Value |
| --- | --- |
| `ConnectionStrings__mowtrackr` | the Azure SQL connection string |
| `Jwt__SigningKey` | a long random secret (32+ characters) |
| `Client__BaseUrl` | `https://<your-static-web-app>.azurestaticapps.net` |
| `Authentication__Google__ClientId` / `__ClientSecret` | optional - enables Google sign-in |
| `Authentication__Facebook__ClientId` / `__ClientSecret` | optional - enables Facebook sign-in |
| `Authentication__LinkedIn__ClientId` / `__ClientSecret` | optional - enables LinkedIn sign-in |

Register each provider's OAuth redirect URI as `https://<your-api>.azurewebsites.net/signin-<provider>`
(`signin-google`, `signin-facebook`, `signin-linkedin`).

The API applies EF Core migrations on startup, so the first request creates the schema.

## 3. Wire up GitHub Actions

In the repository settings add:

| Kind | Name | Value |
| --- | --- | --- |
| Variable | `AZURE_API_APP_NAME` | the App Service name |
| Secret | `AZURE_API_PUBLISH_PROFILE` | the App Service publish profile XML |
| Variable | `AZURE_API_BASE_URL` | `https://<your-api>.azurewebsites.net/` |
| Secret | `AZURE_STATIC_WEB_APPS_API_TOKEN` | the Static Web App deployment token |

Until those exist the deploy workflows skip themselves; CI (build) always runs.