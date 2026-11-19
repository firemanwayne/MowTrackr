using '../main.bicep'

param environment = 'dev'
// West US 2: the subscription's F1 (free) App Service quota is only available there.
param location = 'westus2'
param staticWebAppLocation = 'westus2'
// SQL provisioning is restricted in West US 2 and East US 2 for this subscription; West US 3 is open.
param sqlLocation = 'westus3'
param instance = '001'

// The client's custom domain (a CNAME in the shared mowtrackr.com zone).
param clientHostName = 'dev.mowtrackr.com'

// Everything free: dev takes the subscription's one free SQL database.
param appServicePlanSku = 'F1'
param sqlDatabaseSkuName = 'GP_S_Gen5_2'
param sqlDatabaseSkuCapacity = 2
param sqlUseFreeLimit = true

// Secrets come from the deploy workflow's environment variables (GitHub environment secrets).
param jwtSigningKey = readEnvironmentVariable('JWT_SIGNING_KEY', '')
param smsSenderNumber = readEnvironmentVariable('SMS_SENDER_NUMBER', '')
param googleClientId = readEnvironmentVariable('GOOGLE_CLIENT_ID', '')
param googleClientSecret = readEnvironmentVariable('GOOGLE_CLIENT_SECRET', '')
param facebookClientId = readEnvironmentVariable('FACEBOOK_CLIENT_ID', '')
param facebookClientSecret = readEnvironmentVariable('FACEBOOK_CLIENT_SECRET', '')
param linkedInClientId = readEnvironmentVariable('LINKEDIN_CLIENT_ID', '')
param linkedInClientSecret = readEnvironmentVariable('LINKEDIN_CLIENT_SECRET', '')
