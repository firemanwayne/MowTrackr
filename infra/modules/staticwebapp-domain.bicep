// Binds a custom domain to the Static Web App. The CNAME record must already exist (validated by CNAME delegation);
// Azure issues and renews the certificate.

@description('Static Web App name.')
param staticWebAppName string

@description('Custom host name, e.g. dev.mowtrackr.com.')
param domainName string

resource site 'Microsoft.Web/staticSites@2023-12-01' existing = {
  name: staticWebAppName
}

resource customDomain 'Microsoft.Web/staticSites/customDomains@2023-12-01' = {
  parent: site
  name: domainName
  properties: {
    validationMethod: 'cname-delegation'
  }
}
