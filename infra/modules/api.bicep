// API hosting: a Linux App Service plan and the web app that runs the .NET 10 API, with a system-assigned
// managed identity (used for SQL and Key Vault).

@description('Region for the plan and web app.')
param location string

@description('App Service plan name.')
param planName string

@description('Globally unique web app name (becomes <name>.azurewebsites.net).')
param appName string

@description('Plan SKU. F1 is free (no always-on); B1 and above are paid.')
@allowed([
  'F1'
  'B1'
  'B2'
  'P0v3'
])
param planSku string = 'F1'

@description('Application settings (name/value). Double underscore stands for ":" in configuration keys.')
param appSettings object

@description('Resource tags.')
param tags object = {}

var isFree = planSku == 'F1'

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: planName
  location: location
  tags: tags
  kind: 'linux'
  sku: {
    name: planSku
    tier: isFree ? 'Free' : (startsWith(planSku, 'B') ? 'Basic' : 'PremiumV3')
  }
  properties: {
    reserved: true
  }
}

resource app 'Microsoft.Web/sites@2023-12-01' = {
  name: appName
  location: location
  tags: tags
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: !isFree
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      appSettings: [for setting in items(appSettings): {
        name: setting.key
        value: string(setting.value)
      }]
    }
  }
}

@description('Web app name.')
output name string = app.name

@description('Default host name of the web app.')
output defaultHostname string = app.properties.defaultHostName

@description('Object id of the web app managed identity.')
output principalId string = app.identity.principalId
