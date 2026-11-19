// Azure Communication Services for sending email: an Email Service with a sender domain, linked to a Communication
// Services resource. Mail is sent from invitations@<domain>.
//
// Two sender domains:
//   - the Azure-managed domain (xxxx.azurecomm.net): always present, works immediately, but mail from it often lands in spam;
//   - a custom domain (e.g. mowtrackr.com): authenticated with SPF and DKIM records in our own DNS zone, so it is trusted.
// ACS only lets a custom domain be linked once its DNS records are verified, which happens after the records exist and
// Azure has been asked to check them (infra/scripts/email-domain.sh). Until then the managed domain is used.

@description('Communication Services resource name.')
param name string

@description('Email Services resource name.')
param emailServiceName string

@description('Where the data is stored.')
@allowed([
  'United States'
  'Europe'
  'UK'
  'Australia'
])
param dataLocation string = 'United States'

@description('Custom sender domain, for example mowtrackr.com or dev.mowtrackr.com. Empty to use only the Azure-managed domain.')
param customEmailDomain string = ''

@description('Link the custom domain so mail is sent from it. Only valid once the domain is verified; the deploy workflow sets this.')
param linkCustomEmailDomain bool = false

@description('Local part of the sender address.')
param senderUsername string = 'invitations'

@description('Name shown to recipients next to the address.')
param senderDisplayName string = 'MowTrackr'

@description('Resource tags.')
param tags object = {}

var useCustomDomain = !empty(customEmailDomain)

// Both resource types are global.
resource emailService 'Microsoft.Communication/emailServices@2023-04-01' = {
  name: emailServiceName
  location: 'global'
  tags: tags
  properties: {
    dataLocation: dataLocation
  }
}

resource managedDomain 'Microsoft.Communication/emailServices/domains@2023-04-01' = {
  parent: emailService
  name: 'AzureManagedDomain'
  location: 'global'
  tags: tags
  properties: {
    domainManagement: 'AzureManaged'
    userEngagementTracking: 'Disabled'
  }
}

resource managedSender 'Microsoft.Communication/emailServices/domains/senderUsernames@2023-04-01' = {
  parent: managedDomain
  name: senderUsername
  properties: {
    username: senderUsername
    displayName: senderDisplayName
  }
}

resource customDomain 'Microsoft.Communication/emailServices/domains@2023-04-01' = if (useCustomDomain) {
  parent: emailService
  name: customEmailDomain
  location: 'global'
  tags: tags
  properties: {
    domainManagement: 'CustomerManaged'
    userEngagementTracking: 'Disabled'
  }
}

resource customSender 'Microsoft.Communication/emailServices/domains/senderUsernames@2023-04-01' = if (useCustomDomain) {
  parent: customDomain
  name: senderUsername
  properties: {
    username: senderUsername
    displayName: senderDisplayName
  }
}

var sendFromCustomDomain = useCustomDomain && linkCustomEmailDomain

#disable-next-line BCP318
var linkedDomainIds = sendFromCustomDomain ? [managedDomain.id, customDomain.id] : [managedDomain.id]

resource communicationService 'Microsoft.Communication/communicationServices@2023-04-01' = {
  name: name
  location: 'global'
  tags: tags
  properties: {
    dataLocation: dataLocation
    linkedDomains: linkedDomainIds
  }
  dependsOn: [
    managedSender
  ]
}

@description('The address mail is sent from.')
output senderAddress string = sendFromCustomDomain ? '${senderUsername}@${customEmailDomain}' : '${senderUsername}@${managedDomain.properties.fromSenderDomain}'

@description('DNS records Azure needs on the custom domain (empty when there is no custom domain).')
#disable-next-line BCP318
output verificationRecords object = useCustomDomain ? customDomain.properties.verificationRecords : {}

@description('Connection string of the Communication Services resource (a secret: store it in Key Vault).')
@secure()
output connectionString string = communicationService.listKeys().primaryConnectionString
