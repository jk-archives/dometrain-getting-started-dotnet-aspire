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
    .WaitFor(cache)
    .WithHttpCommand("/reset", "Reset ratings", commandOptions: new()
    {
        IconName = "Delete"
    });

var api = builder.AddProject<Api>("api")
    .WithReference(db)
    .WithReference(ratingService)
    .WaitFor(db)
    .WaitFor(ratingService)
    .WithCommand("say-hello", "Say hello!", context =>
    {
        Console.WriteLine("Hello!");

        return Task.FromResult(new ExecuteCommandResult { Success = true, });
    }, commandOptions: new()
    {
        IconName = "EmojiSmileSlight",
    });

builder.AddProject<Frontend>("frontend")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

builder.AddProject<MigrationService>("migration")
    .WithReference(db)
    .WaitFor(db)
    .WithParentRelationship(server);

builder.Build().Run();