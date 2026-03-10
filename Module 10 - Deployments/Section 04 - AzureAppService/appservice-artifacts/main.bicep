targetScope = 'subscription'

param resourceGroupName string

param location string

param principalId string

resource rg 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
}

module app_service_env_acr 'app-service-env-acr/app-service-env-acr.bicep' = {
  name: 'app-service-env-acr'
  scope: rg
  params: {
    location: location
  }
}

module app_service_env 'app-service-env/app-service-env.bicep' = {
  name: 'app-service-env'
  scope: rg
  params: {
    location: location
    app_service_env_acr_outputs_name: app_service_env_acr.outputs.name
    userPrincipalId: principalId
  }
}

module server 'server/server.bicep' = {
  name: 'server'
  scope: rg
  params: {
    location: location
  }
}

module cache 'cache/cache.bicep' = {
  name: 'cache'
  scope: rg
  params: {
    location: location
  }
}

module ratingservice_identity 'ratingservice-identity/ratingservice-identity.bicep' = {
  name: 'ratingservice-identity'
  scope: rg
  params: {
    location: location
  }
}

module ratingservice_roles_cache 'ratingservice-roles-cache/ratingservice-roles-cache.bicep' = {
  name: 'ratingservice-roles-cache'
  scope: rg
  params: {
    location: location
    cache_outputs_name: cache.outputs.name
    principalId: ratingservice_identity.outputs.principalId
    principalName: ratingservice_identity.outputs.principalName
  }
}

module api_identity 'api-identity/api-identity.bicep' = {
  name: 'api-identity'
  scope: rg
  params: {
    location: location
  }
}

module api_roles_server 'api-roles-server/api-roles-server.bicep' = {
  name: 'api-roles-server'
  scope: rg
  params: {
    location: location
    server_outputs_name: server.outputs.name
    server_outputs_sqlserveradminname: server.outputs.sqlServerAdminName
    principalId: api_identity.outputs.principalId
    principalName: api_identity.outputs.principalName
  }
}

module migration_identity 'migration-identity/migration-identity.bicep' = {
  name: 'migration-identity'
  scope: rg
  params: {
    location: location
  }
}

module migration_roles_server 'migration-roles-server/migration-roles-server.bicep' = {
  name: 'migration-roles-server'
  scope: rg
  params: {
    location: location
    server_outputs_name: server.outputs.name
    server_outputs_sqlserveradminname: server.outputs.sqlServerAdminName
    principalId: migration_identity.outputs.principalId
    principalName: migration_identity.outputs.principalName
  }
}

output app_service_env_AZURE_CONTAINER_REGISTRY_ENDPOINT string = app_service_env.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT

output app_service_env_planId string = app_service_env.outputs.planId

output app_service_env_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID string = app_service_env.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_ID

output app_service_env_AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_CLIENT_ID string = app_service_env.outputs.AZURE_CONTAINER_REGISTRY_MANAGED_IDENTITY_CLIENT_ID

output cache_connectionString string = cache.outputs.connectionString

output ratingservice_identity_id string = ratingservice_identity.outputs.id

output ratingservice_identity_clientId string = ratingservice_identity.outputs.clientId

output app_service_env_AZURE_APP_SERVICE_DASHBOARD_URI string = app_service_env.outputs.AZURE_APP_SERVICE_DASHBOARD_URI

output app_service_env_AZURE_WEBSITE_CONTRIBUTOR_MANAGED_IDENTITY_ID string = app_service_env.outputs.AZURE_WEBSITE_CONTRIBUTOR_MANAGED_IDENTITY_ID

output app_service_env_AZURE_WEBSITE_CONTRIBUTOR_MANAGED_IDENTITY_PRINCIPAL_ID string = app_service_env.outputs.AZURE_WEBSITE_CONTRIBUTOR_MANAGED_IDENTITY_PRINCIPAL_ID

output server_sqlServerFqdn string = server.outputs.sqlServerFqdn

output api_identity_id string = api_identity.outputs.id

output api_identity_clientId string = api_identity.outputs.clientId

output migration_identity_id string = migration_identity.outputs.id

output migration_identity_clientId string = migration_identity.outputs.clientId