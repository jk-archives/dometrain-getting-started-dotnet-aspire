$projects = @(
    @{ Path = "./Api/Api.csproj"; Image = "podcasts-api" },
    @{ Path = "./Frontend/Frontend.csproj"; Image = "podcasts-frontend" },
    @{ Path = "./RatingService/RatingService.csproj"; Image = "podcasts-ratingservice" },
    @{ Path = "./MigrationService/MigrationService.csproj"; Image = "podcasts-migration" }
)

foreach ($project in $projects) {
    Write-Host "Building $($project.Image)..." -ForegroundColor Cyan
    dotnet publish $project.Path `
        --os linux --arch x64 `
        /t:PublishContainer `
        /p:ContainerRepository=$($project.Image) `
        /p:ContainerImageTag=latest
    if ($LASTEXITCODE -ne 0) { throw "Failed to build $($project.Image)" }
}

Write-Host "All images built successfully." -ForegroundColor Green
