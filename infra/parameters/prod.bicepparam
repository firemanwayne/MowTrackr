using '../main.bicep'

param environment = 'prod'
param location = 'eastus2'
param staticWebAppLocation = 'eastus2'
param instance = '001'

// The free SQL offer is taken by dev, so prod runs a small paid serverless database (1 vCore, auto-pauses when idle).
// The plan stays on F1 (free); move to B1 for always-on once the app is in regular use.
param appServicePlanSku = 'F1'
param sqlDatabaseSkuName = 'GP_S_Gen5_1'
param sqlDatabaseSkuCapacity = 1
param sqlUseFreeLimit = false

param jwtSigningKey = readEnvironmentVariable('JWT_SIGNING_KEY', '')
param googleClientId = readEnvironmentVariable('GOOGLE_CLIENT_ID', '')
param googleClientSecret = readEnvironmentVariable('GOOGLE_CLIENT_SECRET', '')
param facebookClientId = readEnvironmentVariable('FACEBOOK_CLIENT_ID', '')
param facebookClientSecret = readEnvironmentVariable('FACEBOOK_CLIENT_SECRET', '')
param linkedInClientId = readEnvironmentVariable('LINKEDIN_CLIENT_ID', '')
param linkedInClientSecret = readEnvironmentVariable('LINKEDIN_CLIENT_SECRET', '')
