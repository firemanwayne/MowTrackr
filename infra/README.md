# infra

Infrastructure as code for Azure, written in Bicep, and the scripts the deploy uses.
The full walkthrough (one-time setup, DNS, email, secrets) is in [docs/azure-setup.md](../docs/azure-setup.md).

| Path | What it is |
| --- | --- |
| `main.bicep` | One environment (resource-group scope): SQL, Key Vault, the API on App Service and the client on a Static Web App. |
| `shared.bicep` | Resources shared by every environment: the Azure DNS zone for `mowtrackr.com`. Deployed once by hand. |
| `parameters/` | Per-environment values: `dev.bicepparam` and `prod.bicepparam`. |
| `modules/api.bicep` | App Service plan and the API web app, with a managed identity. |
| `modules/sql.bicep` | Azure SQL server (Microsoft Entra-only sign-in) and database. |
| `modules/key-vault.bicep` | Key Vault holding the API's secrets, read through Key Vault references. |
| `modules/staticwebapp.bicep` | The Static Web App that serves the client. |
| `modules/staticwebapp-domain.bicep`, `modules/dns-cname.bicep` | The custom domain and its DNS record. |
| `modules/communication.bicep`, `modules/dns-email-records.bicep` | Azure Communication Services email and the SPF, DKIM and DMARC records. |
| `scripts/apply-csp.py` | Adds a Content-Security-Policy to the published client, hashing its inline scripts. |
| `scripts/email-domain.sh` | Checks and verifies the custom email domain's DNS records. |
| `bootstrap.ps1` | One-time setup: resource groups, the GitHub OIDC sign-in to Azure, and the GitHub environments, variables and secrets. |

## How it is deployed

The `Deploy` workflow (`.github/workflows/deploy.yml`, which calls `deploy-environment.yml` once per environment) runs
these templates. Merging to `main` deploys the dev environment, then waits for approval (the `prod-approval`
environment) before deploying prod. A manual run can pick `dev` or `prod`; prod runs only from `main` and also needs
the approval. Tick *what-if* on a manual run to preview changes without applying them.

## Working on it

- CI validates every template and parameter file, so check a change locally first:

  ```bash
  az bicep build --file infra/main.bicep --stdout > /dev/null
  az bicep build-params --file infra/parameters/dev.bicepparam --stdout > /dev/null
  ```

- Never run a manual `az deployment` with placeholder secrets: they would overwrite the real ones in Key Vault. The
  `JWT_SIGNING_KEY` GitHub environment secret is the source of truth.
- Resource names follow `<app>-<resource>-<env>-<instance>`. App Service and Key Vault names are global, so change
  `instance` in the parameter file if a name is taken.
