// Main deployment file that orchestrates all Azure resources
// for the Expense Management System

targetScope = 'subscription'

@description('Location for all resources')
param location string = 'uksouth'

@description('Base name for resources')
param baseName string = 'expensemgmt'

@description('Environment suffix')
param environment string = 'dev'

@description('Resource group name')
param resourceGroupName string = 'rg-${baseName}-${environment}'

@description('Deploy GenAI resources (set to false to skip Chat UI infrastructure)')
param includeChatUI bool = false

@description('Timestamp for unique naming')
param timestamp string = utcNow('ddHHmm')

// Resource Group
resource resourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
}

// Managed Identity
module managedIdentity 'managed-identity.bicep' = {
  scope: resourceGroup
  name: 'managedIdentityDeployment'
  params: {
    location: location
    timestamp: timestamp
  }
}

// App Service
module appService 'app-service.bicep' = {
  scope: resourceGroup
  name: 'appServiceDeployment'
  params: {
    location: location
    baseName: baseName
    environment: environment
    timestamp: timestamp
    managedIdentityClientId: managedIdentity.outputs.managedIdentityClientId
    managedIdentityId: managedIdentity.outputs.managedIdentityId
  }
}

// GenAI Resources (conditional)
module genai 'genai-resources.bicep' = {
  scope: resourceGroup
  name: 'genaiDeployment'
  params: {
    location: location
    baseName: baseName
    environment: environment
    includeChatUI: includeChatUI
    managedIdentityId: managedIdentity.outputs.managedIdentityId
  }
}

output resourceGroupName string = resourceGroup.name
output appServiceName string = appService.outputs.appServiceName
output appServiceUrl string = appService.outputs.appServiceUrl
output managedIdentityName string = managedIdentity.outputs.managedIdentityName
output managedIdentityClientId string = managedIdentity.outputs.managedIdentityClientId
output openAIEndpoint string = genai.outputs.openAIEndpoint
output searchEndpoint string = genai.outputs.searchEndpoint
output gpt4oDeploymentName string = genai.outputs.gpt4oDeploymentName
