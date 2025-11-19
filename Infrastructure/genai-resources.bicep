// Azure OpenAI and Cognitive Services Infrastructure
// Deploys Azure OpenAI with GPT-4o model for Chat UI functionality

@description('Location for all resources')
param location string = 'uksouth'

@description('Base name for resources')
param baseName string = 'expensemgmt'

@description('Environment suffix')
param environment string = 'dev'

@description('Deploy GenAI resources')
param includeChatUI bool = false

@description('User Assigned Managed Identity Resource ID')
param managedIdentityId string

// Azure OpenAI Account
resource openAIAccount 'Microsoft.CognitiveServices/accounts@2023-05-01' = if (includeChatUI) {
  name: 'aoai-${baseName}-${environment}'
  location: 'swedencentral'  // GPT-4o available in Sweden
  kind: 'OpenAI'
  sku: {
    name: 'S0'  // Standard S0 SKU for low-cost development
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    customSubDomainName: 'aoai-${baseName}-${environment}-${uniqueString(resourceGroup().id)}'
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
    }
  }
}

// Deploy GPT-4o model
resource gpt4oDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = if (includeChatUI) {
  parent: openAIAccount
  name: 'gpt-4o'
  sku: {
    name: 'Standard'
    capacity: 10
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
      version: '2024-05-13'
    }
    versionUpgradeOption: 'OnceNewDefaultVersionAvailable'
  }
}

// Azure AI Search for RAG (low-cost tier)
resource searchService 'Microsoft.Search/searchServices@2023-11-01' = if (includeChatUI) {
  name: 'srch-${baseName}-${environment}'
  location: location
  sku: {
    name: 'basic'  // Basic tier for low-cost development
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    replicaCount: 1
    partitionCount: 1
    hostingMode: 'default'
    publicNetworkAccess: 'enabled'
  }
}

output openAIEndpoint string = includeChatUI ? openAIAccount.properties.endpoint : ''
output openAIName string = includeChatUI ? openAIAccount.name : ''
output openAIId string = includeChatUI ? openAIAccount.id : ''
output searchEndpoint string = includeChatUI ? 'https://${searchService.name}.search.windows.net' : ''
output searchName string = includeChatUI ? searchService.name : ''
output gpt4oDeploymentName string = includeChatUI ? gpt4oDeployment.name : ''
