// Azure SQL logical server (Microsoft Entra-only authentication, no SQL logins) plus one serverless database.
// The Entra administrator is the API's managed identity, so the API can run EF Core migrations and use the database
// without any password.

@description('Region for the SQL server.')
param location string

@description('Globally unique SQL server name.')
param serverName string

@description('Database name.')
param databaseName string = 'mowtrackr'

@description('Display name of the Entra administrator (the API web app name).')
param adminLogin string

@description('Object id of the Entra administrator (the API managed identity principal id).')
param adminObjectId string

@description('Entra tenant id.')
param tenantId string = subscription().tenantId

@description('Database SKU name, e.g. GP_S_Gen5_2 (serverless).')
param skuName string = 'GP_S_Gen5_2'

@description('Database vCore capacity.')
param skuCapacity int = 2

@description('Use the free database offer (one per subscription). It auto-pauses when the monthly free allowance runs out.')
param useFreeLimit bool = false

@description('Auto-pause delay in minutes for the serverless database.')
param autoPauseDelay int = 60

@description('Resource tags.')
param tags object = {}

resource server 'Microsoft.Sql/servers@2024-05-01-preview' = {
  name: serverName
  location: location
  tags: tags
  properties: {
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      login: adminLogin
      principalType: 'Application'
      sid: adminObjectId
      tenantId: tenantId
    }
  }
}

// 0.0.0.0 - 0.0.0.0 is the "Allow Azure services" rule, which lets the App Service reach the server.
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2024-05-01-preview' = {
  parent: server
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource database 'Microsoft.Sql/servers/databases@2024-05-01-preview' = {
  parent: server
  name: databaseName
  location: location
  tags: tags
  sku: {
    name: skuName
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: skuCapacity
  }
  properties: {
    useFreeLimit: useFreeLimit
    freeLimitExhaustionBehavior: useFreeLimit ? 'AutoPause' : null
    autoPauseDelay: autoPauseDelay
    minCapacity: json('0.5')
    requestedBackupStorageRedundancy: 'Local'
  }
}

@description('Fully qualified domain name of the SQL server.')
output serverFqdn string = server.properties.fullyQualifiedDomainName

@description('Name of the database.')
output databaseName string = database.name
