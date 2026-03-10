# Incrememnt this if you make code changes, and you want to force Kubernetes to pull the fresh image
$imageTag = '2'

# For the demo, just use the same password for everything
$password = 'fdsafsd23423#123'

# Publish (ie. create the Helm template)
Write-Output 'aspire publish...'
pause
aspire publish

# Build the docker images
Write-Output 'aspire do build...'
pause
aspire do build

# Tag the built docker images
Write-Output 'Tag images...'
pause
docker tag api:latest api:$imageTag
docker tag frontend:latest frontend:$imageTag
docker tag ratingservice:latest ratingservice:$imageTag
docker tag migration:latest migration:$imageTag

# Install into your cluster
Write-Output 'Helm install...'
pause
helm upgrade `
  --install `
  --wait `
  --namespace podcast `
  --create-namespace `
  --set secrets.ratingservice.cache_password="$password" `
  --set secrets.migration.password="$password" `
  --set secrets.api.password="$password" `
  --set secrets.server.password="$password" `
  --set secrets.server.MSSQL_SA_PASSWORD="$password" `
  --set secrets.cache.REDIS_PASSWORD="$password" `
  --set secrets.cache.cache_password="$password" `
  --set parameters.ratingservice.ratingservice_image="ratingservice:$imageTag" `
  --set parameters.frontend.frontend_image="frontend:$imageTag" `
  --set parameters.api.api_image="api:$imageTag" `
  --set parameters.migration.migration_image="migration:$imageTag" `
  podcast .\AppHost\aspire-output\.

# Port-forward, so we can access the front-end service
Write-Output 'Port forward...'
pause
kubectl port-forward --namespace podcast service/frontend-service 1234:8080

