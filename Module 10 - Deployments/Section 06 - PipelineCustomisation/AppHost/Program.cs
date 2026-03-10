using AppHost;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddKubernetesEnvironment("k8s");

if (builder.ExecutionContext.IsPublishMode)
    PipelineSteps.AddCustomPipelineSteps(builder);

var password = builder.AddParameter("password", secret: true);

var server = builder.AddSqlServer("server", password, 1433)
    .WithLifetime(ContainerLifetime.Persistent);

var db = server
    .AddDatabase("podcasts");

var cache = builder.AddRedis("cache")
    .WithRedisCommander()
    .WithLifetime(ContainerLifetime.Persistent);

var ratingService = builder.AddProject<RatingService>("ratingservice", launchProfileName: null)
    .WithReference(cache)
    .WaitFor(cache)
    .WithHttpEndpoint();

var api = builder.AddProject<Api>("api", launchProfileName: null)
    .WithReference(db)
    .WithReference(ratingService)
    .WaitFor(db)
    .WaitFor(ratingService)
    .WithHttpEndpoint();

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
