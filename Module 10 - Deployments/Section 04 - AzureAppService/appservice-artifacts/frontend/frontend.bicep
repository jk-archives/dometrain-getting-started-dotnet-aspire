@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

param app_service_env_outputs_azure_container_registry_endpoint string

param app_service_env_outputs_planid string

param app_service_env_outputs_azure_container_registry_managed_identity_id string

param app_service_env_outputs_azure_container_registry_managed_identity_client_id string

param frontend_containerimage string

param frontend_containerport string

param app_service_env_outputs_azure_app_service_dashboard_uri string

param app_service_env_outputs_azure_website_contributor_managed_identity_id string

param app_service_env_outputs_azure_website_contributor_managed_identity_principal_id string

resource mainContainer 'Microsoft.Web/sites/sitecontainers@2025-03-01' = {
  name: 'main'
  properties: {
    authType: 'UserAssigned'
    image: frontend_containerimage
    isMain: true
    targetPort: frontend_containerport
    userManagedIdentityClientId: app_service_env_outputs_azure_container_registry_managed_identity_client_id
  }
  parent: webapp
}

resource webapp 'Microsoft.Web/sites@2025-03-01' = {
  name: take('${toLower('frontend')}-${uniqueString(resourceGroup().id)}', 60)
  location: location
  properties: {
    serverFarmId: app_service_env_outputs_planid
    siteConfig: {
      numberOfWorkers: 30
      linuxFxVersion: 'SITECONTAINERS'
      acrUseManagedIdentityCreds: true
      acrUserManagedIdentityID: app_service_env_outputs_azure_container_registry_managed_identity_client_id
      appSettings: [
        {
          name: 'WEBSITES_PORT'
          value: frontend_containerport
        }
        {
          name: 'OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY'
          value: 'in_memory'
        }
        {
          name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
          value: 'true'
        }
        {
          name: 'HTTP_PORTS'
          value: frontend_containerport
        }
        {
          name: 'API_HTTP'
          value: 'http://${take('${toLower('api')}-${uniqueString(resourceGroup().id)}', 60)}.azurewebsites.net'
        }
        {
          name: 'services__api__http__0'
          value: 'http://${take('${toLower('api')}-${uniqueString(resourceGroup().id)}', 60)}.azurewebsites.net'
        }
        {
          name: 'API_HTTPS'
          value: 'https://${take('${toLower('api')}-${uniqueString(resourceGroup().id)}', 60)}.azurewebsites.net'
        }
        {
          name: 'services__api__https__0'
          value: 'https://${take('${toLower('api')}-${uniqueString(resourceGroup().id)}', 60)}.azurewebsites.net'
        }
        {
          name: 'ASPIRE_ENVIRONMENT_NAME'
          value: 'app-service-env'
        }
        {
          name: 'OTEL_SERVICE_NAME'
          value: 'frontend'
        }
        {
          name: 'OTEL_EXPORTER_OTLP_PROTOCOL'
          value: 'grpc'
        }
        {
          name: 'OTEL_EXPORTER_OTLP_ENDPOINT'
          value: 'http://localhost:6001'
        }
        {
          name: 'WEBSITE_ENABLE_ASPIRE_OTEL_SIDECAR'
          value: 'true'
        }
        {
          name: 'OTEL_COLLECTOR_URL'
          value: app_service_env_outputs_azure_app_service_dashboard_uri
        }
        {
          name: 'OTEL_CLIENT_ID'
          value: app_service_env_outputs_azure_container_registry_managed_identity_client_id
        }
      ]
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${app_service_env_outputs_azure_container_registry_managed_identity_id}': { }
    }
  }
}

resource frontend_website_ra 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(webapp.id, app_service_env_outputs_azure_website_contributor_managed_identity_id, subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'de139f84-1756-47ae-9be6-808fbbe84772'))
  properties: {
    principalId: app_service_env_outputs_azure_website_contributor_managed_identity_principal_id
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'de139f84-1756-47ae-9be6-808fbbe84772')
    principalType: 'ServicePrincipal'
  }
  scope: webapp
}

resource slotConfigNames 'Microsoft.Web/sites/config@2025-03-01' = {
  name: 'slotConfigNames'
  properties: {
    appSettingNames: [
      'API_HTTP'
      'services__api__http__0'
      'API_HTTPS'
      'services__api__https__0'
      'OTEL_SERVICE_NAME'
    ]
  }
  parent: webapp
}