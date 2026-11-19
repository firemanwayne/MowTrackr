// Key Vault (RBAC authorization) holding the API's secrets. The API reads them through Key Vault references
// in its app settings, using its managed identity (role assignment made in main.bicep).

@description('Globally unique Key Vault name (3-24 characters).')
@minLength(3)
@maxLength(24)
param name string

@description('Region for the vault.')
param location string

@description('Secrets to store, as name/value. Entries with an empty value are skipped.')
@secure()
param secrets object

@description('Soft delete retention in days.')
@minValue(7)
@maxValue(90)
param softDeleteRetentionInDays int = 7

@description('Resource tags.')
param tags object = {}

resource vault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: softDeleteRetentionInDays
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
  }
}

resource secret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = [for item in items(secrets): if (!empty(item.value)) {
  parent: vault
  name: item.key
  properties: {
    value: string(item.value)
  }
}]

@description('Key Vault name.')
output name string = vault.name
