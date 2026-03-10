$SqlPassword = "SqlP@ssw0rd2025!x"
$RedisPassword = "R3dis!Str0ngK3y#9"

helm uninstall apphost --namespace blah --ignore-not-found

helm install apphost .\k8s-artifacts `
    --namespace blah `
    --create-namespace `
    --set secrets.server.password=$SqlPassword `
    --set secrets.api.password=$SqlPassword `
    --set secrets.migration.password=$SqlPassword `
    --set secrets.cache.cache_password=$RedisPassword `
    --set secrets.ratingservice.cache_password=$RedisPassword
