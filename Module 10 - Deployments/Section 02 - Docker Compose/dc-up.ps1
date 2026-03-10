$SqlPassword = "SqlP@ssw0rd2025!x"
$RedisPassword = "R3dis!Str0ngK3y#9"

docker compose --env-file ./dc-artifacts/.env -f ./dc-artifacts/docker-compose.yaml down --remove-orphans 2>$null

$env:PASSWORD = $SqlPassword
$env:CACHE_PASSWORD = $RedisPassword

docker compose --env-file ./dc-artifacts/.env -f ./dc-artifacts/docker-compose.yaml up

Write-Host "Docker Compose deployment started. Use 'docker compose logs -f' to follow logs." -ForegroundColor Green
