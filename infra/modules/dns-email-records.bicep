// DNS records for a custom Communication Services email domain, created in the shared DNS zone (deployed into the
// zone's resource group): ownership and SPF (TXT), two DKIM keys (CNAME) and a DMARC policy.

@description('DNS zone name, e.g. mowtrackr.com.')
param zoneName string

@description('The email domain: the zone itself (apex) or a subdomain of it, e.g. dev.mowtrackr.com.')
param emailDomain string

@description('The verificationRecords Azure returned for the email domain (Domain, SPF, DKIM and DKIM2).')
param records object

@description('DMARC policy for the domain.')
param dmarcPolicy string = 'v=DMARC1; p=quarantine; adkim=s; aspf=s; pct=100'

var isApex = emailDomain == zoneName

// For a subdomain such as dev.mowtrackr.com the label is "dev"; records are named under it.
var label = isApex ? '' : substring(emailDomain, 0, length(emailDomain) - length(zoneName) - 1)
var suffix = isApex ? '' : '.${label}'

resource zone 'Microsoft.Network/dnsZones@2018-05-01' existing = {
  name: zoneName
}

// Ownership and SPF are both TXT records on the domain itself, so they share one record set.
resource txt 'Microsoft.Network/dnsZones/TXT@2018-05-01' = {
  parent: zone
  name: isApex ? '@' : label
  properties: {
    TTL: 3600
    TXTRecords: [
      {
        value: [
          records.Domain.value
        ]
      }
      {
        value: [
          records.SPF.value
        ]
      }
    ]
  }
}

resource dkim 'Microsoft.Network/dnsZones/CNAME@2018-05-01' = {
  parent: zone
  name: '${records.DKIM.name}${suffix}'
  properties: {
    TTL: 3600
    CNAMERecord: {
      cname: records.DKIM.value
    }
  }
}

resource dkim2 'Microsoft.Network/dnsZones/CNAME@2018-05-01' = {
  parent: zone
  name: '${records.DKIM2.name}${suffix}'
  properties: {
    TTL: 3600
    CNAMERecord: {
      cname: records.DKIM2.value
    }
  }
}

resource dmarc 'Microsoft.Network/dnsZones/TXT@2018-05-01' = {
  parent: zone
  name: '_dmarc${suffix}'
  properties: {
    TTL: 3600
    TXTRecords: [
      {
        value: [
          dmarcPolicy
        ]
      }
    ]
  }
}
