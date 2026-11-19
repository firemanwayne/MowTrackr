// A CNAME record in the shared DNS zone (deployed into the zone's resource group).

@description('DNS zone name, e.g. mowtrackr.com.')
param zoneName string

@description('Record name relative to the zone, e.g. "dev" for dev.mowtrackr.com.')
param recordName string

@description('Host name the record points to.')
param target string

resource zone 'Microsoft.Network/dnsZones@2018-05-01' existing = {
  name: zoneName
}

resource cname 'Microsoft.Network/dnsZones/CNAME@2018-05-01' = {
  parent: zone
  name: recordName
  properties: {
    TTL: 3600
    CNAMERecord: {
      cname: target
    }
  }
}
