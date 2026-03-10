$projects = @(
    @{ Path = "./Api/Api.csproj"; Image = "api" },
    @{ Path = "./Frontend/Frontend.csproj"; Image = "frontend" },
    @{ Path = "./RatingService/RatingService.csproj"; Image = "ratingservice" },
    @{ Path = "./MigrationService/MigrationService.csproj"; Image = "migration" }
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
