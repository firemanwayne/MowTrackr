using '../main.bicep'

param environment = 'dev'
param location = 'eastus2'
param staticWebAppLocation = 'eastus2'
param instance = '001'

// Everything free: dev takes the subscription's one free SQL database.
param appServicePlanSku = 'F1'
param sqlDatabaseSkuName = 'GP_S_Gen5_2'
param sqlDatabaseSkuCapacity = 2
param sqlUseFreeLimit = true

// Secrets come from the deploy workflow's environment variables (GitHub environment secrets).
param jwtSigningKey = readEnvironmentVariable('JWT_SIGNING_KEY', '')
param googleClientId = readEnvironmentVariable('GOOGLE_CLIENT_ID', '')
param googleClientSecret = readEnvironmentVariable('GOOGLE_CLIENT_SECRET', '')
param facebookClientId = readEnvironmentVariable('FACEBOOK_CLIENT_ID', '')
param facebookClientSecret = readEnvironmentVariable('FACEBOOK_CLIENT_SECRET', '')
param linkedInClientId = readEnvironmentVariable('LINKEDIN_CLIENT_ID', '')
param linkedInClientSecret = readEnvironmentVariable('LINKEDIN_CLIENT_SECRET', '')
