// Static Web App (free) that serves the Blazor WebAssembly client. Content is uploaded by the deploy workflow.

@description('Region for the Static Web App (only a handful of regions are supported).')
param location string

@description('Static Web App name.')
param name string

@description('Resource tags.')
param tags object = {}

resource site 'Microsoft.Web/staticSites@2023-12-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

@description('Static Web App name.')
output name string = site.name

@description('Default host name of the Static Web App.')
output defaultHostname string = site.properties.defaultHostname
