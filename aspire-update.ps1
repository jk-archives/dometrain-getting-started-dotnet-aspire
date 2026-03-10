$solutions = @(
    "Module 03 - Adding Aspire\Section 01 - Without Aspire"
    "Module 03 - Adding Aspire\Section 02 - AddAppHost"
    "Module 03 - Adding Aspire\Section 03 - AddServiceDefaults"
    "Module 04 - Integrations\Section 02 - Add database"
    "Module 04 - Integrations\Section 03 - Seed database"
    "Module 04 - Integrations\Section 04 - Playing with other integrations"
    "Module 07 - OpenTelemetry\Section 03 - Logs"
    "Module 07 - OpenTelemetry\Section 04 - Metrics"
    "Module 07 - OpenTelemetry\Section 05 - Traces"
    "Module 07 - OpenTelemetry\Section 06 - HealthChecks"
    "Module 08 - Events\Section 01 - Events"
    "Module 09 - Testing\Section 01 - Testing"
    "Module 10 - Deployments\Section 02 - Docker Compose"
    "Module 10 - Deployments\Section 03 - Kubernetes"
    "Module 10 - Deployments\Section 04 - AzureAppService"
    "Module 10 - Deployments\Section 05 - AzureContainerApps"
    "Module 11 - Useful Extras\Section 02 - CustomResourceCommands"
    "Module 11 - Useful Extras\Section 03 - NodeJS"
    "Module 11 - Useful Extras\Section 04 - Docker"
    "Module 11 - Useful Extras\Section 05 - CustomResourceURLs"
)

$root = $PSScriptRoot
$total = $solutions.Count
$current = 0

foreach ($sln in $solutions) {
    $current++
    $path = Join-Path $root $sln
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "[$current/$total] $sln" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan

    Push-Location $path
    try {
        aspire update --channel stable
    }
    finally {
        Pop-Location
    }
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "All done! Run dotnet build on each solution to verify." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
