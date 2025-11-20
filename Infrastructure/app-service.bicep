// App Service Infrastructure for Expense Management System
// Deploys App Service Plan and App Service with User-Assigned Managed Identity

@description('Location for all resources')
param location string = 'uksouth'

@description('Base name for resources')
param baseName string = 'expensemgmt'

@description('Environment suffix')
param environment string = 'dev'

@description('Timestamp for unique naming')
param timestamp string = utcNow('ddHHmm')

@description('User Assigned Managed Identity Client ID')
param managedIdentityClientId string

@description('User Assigned Managed Identity Resource ID')
param managedIdentityId string

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: 'asp-${baseName}-${environment}'
  location: location
  sku: {
    name: 'B1'  // Basic tier - low cost development SKU
    tier: 'Basic'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true  // Required for Linux
  }
}

// App Service
resource appService 'Microsoft.Web/sites@2023-01-01' = {
  name: 'app-${baseName}-${environment}-${timestamp}'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: false  // Not available in Basic tier
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      http20Enabled: true
      appSettings: [
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'ManagedIdentityClientId'
          value: managedIdentityClientId
        }
        {
          name: 'EnableChatUI'
          value: 'true'
        }
      ]
    }
  }
}

output appServiceName string = appService.name
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output appServiceId string = appService.id
