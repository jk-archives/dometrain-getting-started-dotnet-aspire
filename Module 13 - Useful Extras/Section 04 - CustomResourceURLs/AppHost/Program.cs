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

var frontend = builder.AddProject<Frontend>("frontend")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

frontend
    // Customise the existing https endpoint
    .WithUrlForEndpoint("https", x =>
    {
        x.DisplayText = "Frontend (https)";
        x.DisplayOrder = 3;
    })
    // Customise the existing http endpoint
    .WithUrlForEndpoint("http", x =>
    {
        x.DisplayText = "Frontend (http)";
        x.DisplayOrder = 2;
    })
    .WithUrls(ctx =>
    {
        var https = ctx.Urls.First(x => x.Endpoint?.EndpointName == "https");

        // Note: could customise https here if we wanted, but WithUrlForEndpoint is cleaner.

        // Add new "Admin" hyperlink that goes to the admin page
        ctx.Urls.Add(new ResourceUrlAnnotation
        {
            Url = https.Url + "/admin",
            DisplayOrder = 2,
            DisplayText = "Admin"
        });
    })
    // // Another way of setting friendly display names against the existing endpoints
    // .WithUrl($"{frontend.GetEndpoint("https")}", "Frontend (https)")
    // .WithUrl($"{frontend.GetEndpoint("http")}", "Frontend (http)")
    // You can even reference something completed unrelated!
    .WithUrl("https://unhandledexceptionpodcast.com/", "Dan's podcast!");

builder.AddProject<MigrationService>("migration")
    .WithReference(db)
    .WaitFor(db)
    .WithParentRelationship(server);

builder.Build().Run();