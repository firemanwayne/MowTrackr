// Azure Communication Services for sending email: an Email Service with an Azure-managed sender domain, linked to a
// Communication Services resource. (Adapted from the AlertOne repo's communication-services and email-services modules.)

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

@description('Resource tags.')
param tags object = {}

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

resource communicationService 'Microsoft.Communication/communicationServices@2023-04-01' = {
  name: name
  location: 'global'
  tags: tags
  properties: {
    dataLocation: dataLocation
    linkedDomains: [
      managedDomain.id
    ]
  }
}

@description('Sender domain of the Azure-managed email domain, e.g. xxxx.azurecomm.net.')
output senderDomain string = managedDomain.properties.fromSenderDomain

@description('Connection string of the Communication Services resource (a secret: store it in Key Vault).')
@secure()
output connectionString string = communicationService.listKeys().primaryConnectionString
