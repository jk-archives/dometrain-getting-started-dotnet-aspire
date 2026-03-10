@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource migration_identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: take('migration_identity-${uniqueString(resourceGroup().id)}', 128)
  location: location
}

output id string = migration_identity.id

output clientId string = migration_identity.properties.clientId

output principalId string = migration_identity.properties.principalId

output principalName string = migration_identity.name

output name string = migration_identity.name