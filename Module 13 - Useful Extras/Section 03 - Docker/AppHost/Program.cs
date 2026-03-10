using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var password = builder.AddParameter("password", secret: true);

var server = builder.AddSqlServer("server", password, 1433)
    .WithLifetime(ContainerLifetime.Persistent)
    // Example that keeps using the SQL Server integration, but uses our own Dockerfile
    .WithDockerfile(".", "Dockerfile-SqlServer")
    .WithContainerFiles("/", new List<ContainerFileSystemItem>
    {
        new ContainerFile
        {
            Name = "Foo",
            Contents = "Hello!"
        }
    });

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

// Example simple Dockerfile that just creates an nginx webpage saying "Hello Dometrain".
var nginx = builder.AddDockerfile(
        "nginx", ".")
    .WithHttpEndpoint(targetPort: 80);

// Example usage of 3rd-party image (Tetris web-based game in this case)
var tetris = builder.AddContainer("tetris", "uzyexe/tetris")
    .WithHttpEndpoint(targetPort: 80);

var endpoint = tetris.GetEndpoint("http");

builder.AddProject<Frontend>("frontend")
    .WithReference(api)
    .WithReference(endpoint)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

builder.AddProject<MigrationService>("migration")
    .WithReference(db)
    .WaitFor(db)
    .WithParentRelationship(server);

builder.Build().Run();