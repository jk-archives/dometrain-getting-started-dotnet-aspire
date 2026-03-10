using Azure.Provisioning.AppContainers;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAzureContainerAppEnvironment("aca-env")
    .ConfigureInfrastructure(infra =>
    {
        var env = infra.GetProvisionableResources()
            .OfType<ContainerAppManagedEnvironment>()
            .Single();

        // By default, Aspire creates an ACA environment with only a "Consumption" workload profile.
        // Azure treats these as free-tier, and in many regions you'll hit
        // "ManagedEnvironmentCapacityHeavyUsageError" because free-tier capacity is limited.
        //
        // Adding a dedicated workload profile (even with 0 minimum nodes) promotes the
        // environment to paid-tier, which has much better availability across regions.
        // Your apps still run on the default Consumption profile — the D4 profile is just
        // present to unlock paid-tier capacity. You can remove this if your region has
        // free-tier availability, or change the region via `az configure --defaults location=<region>`.
        env.WorkloadProfiles.Add(new ContainerAppWorkloadProfile
        {
            Name = "D4",
            WorkloadProfileType = "D4",
            MinimumNodeCount = 0,
            MaximumNodeCount = 1
        });
    });

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
