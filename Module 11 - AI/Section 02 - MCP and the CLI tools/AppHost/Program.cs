using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("password", secret: true);

var server = builder.AddSqlServer("server", password, 1433)
    .WithLifetime(ContainerLifetime.Persistent);

var db = server
    .AddDatabase("podcasts");

var cache = builder.AddRedis("cache")
    .WithRedisCommander()
    .WithLifetime(ContainerLifetime.Persistent);

var ratingService = builder.AddProject<RatingService>("ratingservice")
    .WithReference(cache)
    .WaitFor(cache);

var api = builder.AddProject<Api>("api")
    .WithReference(db)
    .WithReference(ratingService)
    .WaitFor(db)
    .WaitFor(ratingService);

builder.AddProject<Frontend>("frontend")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

builder.AddProject<MigrationService>("migration")
    .WithReference(db)
    .WaitFor(db)
    .WithParentRelationship(server);

builder.Build().Run();