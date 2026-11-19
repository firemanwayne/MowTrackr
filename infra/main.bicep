// MowTrackr infrastructure for one environment: Azure SQL (Entra-only), Key Vault, the API on App Service
// and the client on Static Web Apps. Deployed at resource-group scope; infra/bootstrap.ps1 creates the groups.
// Per-environment values live in infra/parameters/<environment>.bicepparam.

targetScope = 'resourceGroup'

@description('Deployment environment; used in resource names.')
@allowed([
  'dev'
  'prod'
])
param environment string

@description('Region for the SQL server, plan, web app and Key Vault.')
param location string = resourceGroup().location

@description('Region for the SQL server. Separate because SQL provisioning is restricted in some regions per subscription.')
param sqlLocation string = location

@description('Region for the Static Web App (supported: westus2, centralus, eastus2, westeurope, eastasia).')
param staticWebAppLocation string = 'eastus2'

@description('Instance number appended to resource names (change it if a globally unique name is taken).')
param instance string = '001'

@description('App Service plan SKU (F1 is free).')
param appServicePlanSku string = 'F1'

@description('Database SKU name.')
param sqlDatabaseSkuName string = 'GP_S_Gen5_2'

@description('Database vCore capacity.')
param sqlDatabaseSkuCapacity int = 2

@description('Use the free SQL database offer. Only one database per subscription can.')
param sqlUseFreeLimit bool = false

@description('Signing key for the API-issued JWTs (32+ random characters).')
@secure()
param jwtSigningKey string

@description('Optional Google sign-in client id. Leave empty to hide the provider.')
param googleClientId string = ''

@description('Optional Google sign-in client secret.')
@secure()
param googleClientSecret string = ''

@description('Optional Facebook sign-in app id. Leave empty to hide the provider.')
param facebookClientId string = ''

@description('Optional Facebook sign-in app secret.')
@secure()
param facebookClientSecret string = ''

@description('Optional LinkedIn sign-in client id. Leave empty to hide the provider.')
param linkedInClientId string = ''

@description('Optional LinkedIn sign-in client secret.')
@secure()
param linkedInClientSecret string = ''

var app = 'mowtrackr'
var tags = {
  Application: 'MowTrackr'
  Environment: environment
  ManagedBy: 'Bicep'
}
var sqlServerName = '${app}-sql-${environment}-${instance}'
var databaseName = 'mowtrackr'
var apiName = '${app}-api-${environment}-${instance}'
var vaultName = '${app}-kv-${environment}-${instance}'
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

// Secrets go to Key Vault; the API reads them with Key Vault references.
module vault 'modules/key-vault.bicep' = {
  name: 'key-vault'
  params: {
    name: vaultName
    location: location
    tags: tags
    secrets: {
      'jwt-signing-key': jwtSigningKey
      'google-client-secret': empty(googleClientId) ? '' : googleClientSecret
      'facebook-client-secret': empty(facebookClientId) ? '' : facebookClientSecret
      'linkedin-client-secret': empty(linkedInClientId) ? '' : linkedInClientSecret
    }
  }
}

module client 'modules/staticwebapp.bicep' = {
  name: 'client'
  params: {
    location: staticWebAppLocation
    name: '${app}-swa-${environment}-${instance}'
    tags: tags
  }
}

func secretRef(vault string, secretName string) string => '@Microsoft.KeyVault(VaultName=${vault};SecretName=${secretName})'

var baseSettings = {
  ConnectionStrings__mowtrackr: 'Server=tcp:${sqlServerName}${az.environment().suffixes.sqlServerHostname},1433;Initial Catalog=${databaseName};Authentication=Active Directory Managed Identity;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
  Jwt__SigningKey: secretRef(vaultName, 'jwt-signing-key')
  Client__BaseUrl: 'https://${client.outputs.defaultHostname}'
  ASPNETCORE_ENVIRONMENT: environment == 'dev' ? 'Development' : 'Production'
}
var googleSettings = empty(googleClientId) ? {} : {
  Authentication__Google__ClientId: googleClientId
  Authentication__Google__ClientSecret: secretRef(vaultName, 'google-client-secret')
}
var facebookSettings = empty(facebookClientId) ? {} : {
  Authentication__Facebook__ClientId: facebookClientId
  Authentication__Facebook__ClientSecret: secretRef(vaultName, 'facebook-client-secret')
}
var linkedInSettings = empty(linkedInClientId) ? {} : {
  Authentication__LinkedIn__ClientId: linkedInClientId
  Authentication__LinkedIn__ClientSecret: secretRef(vaultName, 'linkedin-client-secret')
}

module api 'modules/api.bicep' = {
  name: 'api'
  params: {
    location: location
    planName: '${app}-plan-${environment}-${instance}'
    appName: apiName
    planSku: appServicePlanSku
    appSettings: union(baseSettings, googleSettings, facebookSettings, linkedInSettings)
    tags: tags
  }
  dependsOn: [
    vault
  ]
}

// Let the API's managed identity read the vault's secrets.
resource existingVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: vaultName
  dependsOn: [
    vault
  ]
}

resource secretsReader 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(existingVault.id, apiName, keyVaultSecretsUserRoleId)
  scope: existingVault
  properties: {
    principalId: api.outputs.principalId
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
  }
}

// The API's identity is the SQL Entra administrator, so there is no SQL password anywhere.
module sql 'modules/sql.bicep' = {
  name: 'sql'
  params: {
    location: sqlLocation
    serverName: sqlServerName
    databaseName: databaseName
    adminLogin: apiName
    adminObjectId: api.outputs.principalId
    skuName: sqlDatabaseSkuName
    skuCapacity: sqlDatabaseSkuCapacity
    useFreeLimit: sqlUseFreeLimit
    tags: tags
  }
}

@description('Name of the API web app (target of the API deploy).')
output apiName string = api.outputs.name

@description('Base URL of the API, with a trailing slash (the client reads it as ApiBaseUrl).')
output apiUrl string = 'https://${api.outputs.defaultHostname}/'

@description('Name of the Static Web App (target of the client deploy).')
output clientName string = client.outputs.name

@description('Base URL of the client.')
output clientUrl string = 'https://${client.outputs.defaultHostname}'
