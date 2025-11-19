// Managed Identity Infrastructure
// Creates a User-Assigned Managed Identity for the App Service

@description('Location for all resources')
param location string = 'uksouth'

@description('Timestamp for unique naming')
param timestamp string = utcNow('ddHHmm')

// User-Assigned Managed Identity
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'mid-AppModAssist-${timestamp}'
  location: location
}

output managedIdentityName string = managedIdentity.name
output managedIdentityClientId string = managedIdentity.properties.clientId
output managedIdentityId string = managedIdentity.id
