// Resources shared by every environment: the Azure DNS zone for the MowTrackr domain.
// Deployed once at resource-group scope into mowtrackr-rg-shared (created by infra/bootstrap.ps1):
//   az deployment group create --resource-group mowtrackr-rg-shared --template-file infra/shared.bicep
// After the first deployment, point the domain's name servers at the zone (see docs/azure-setup.md).

targetScope = 'resourceGroup'

@description('The domain registered for MowTrackr.')
param zoneName string = 'mowtrackr.com'

@description('Resource tags.')
param tags object = {
  Application: 'MowTrackr'
  ManagedBy: 'Bicep'
}

resource zone 'Microsoft.Network/dnsZones@2018-05-01' = {
  name: zoneName
  location: 'global'
  tags: tags
}

@description('Resource id of the DNS zone.')
output zoneId string = zone.id

@description('Azure DNS name servers for the zone.')
output nameServers array = zone.properties.nameServers
