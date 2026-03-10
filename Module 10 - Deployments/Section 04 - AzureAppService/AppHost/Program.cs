using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureAppServiceEnvironment("app-service-env");

var server = builder.AddAzureSqlServer("server")
    .RunAsContainer(c => c.WithLifetime(ContainerLifetime.Persistent));

var db = server
    .AddDatabase("podcasts");

var cache = builder.AddAzureManagedRedis("cache")
    .RunAsContainer(c => c.WithLifetime(ContainerLifetime.Persistent));

var ratingService = builder.AddProject<RatingService>("ratingservice")
    .WithReference(cache)
    .WaitFor(cache)
    .WithExternalHttpEndpoints();

var api = builder.AddProject<Api>("api")
    .WithReference(db)
    .WithReference(ratingService)
    .WaitFor(db)
    .WaitFor(ratingService)
    .WithExternalHttpEndpoints();

builder.AddProject<Frontend>("frontend")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

builder.AddProject<MigrationService>("migration")
    .WithReference(db)
    .WaitFor(db)
    .WithParentRelationship(server)
    .ExcludeFromManifest();

builder.Build().Run();
