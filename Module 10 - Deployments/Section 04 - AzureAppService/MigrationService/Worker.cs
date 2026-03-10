using System.Diagnostics;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PodcastDbContext>();

            await EnsureDatabaseAsync(dbContext, cancellationToken);
            await RunMigrationAsync(dbContext, cancellationToken);
            await SeedDataAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task EnsureDatabaseAsync(PodcastDbContext dbContext, CancellationToken cancellationToken)
    {
        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Create the database if it does not exist.
            // Do this first so there is then a database to start a transaction against.
            if (!await dbCreator.ExistsAsync(cancellationToken))
            {
                await dbCreator.CreateAsync(cancellationToken);
            }
        });
    }

    private static async Task RunMigrationAsync(PodcastDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Run migration in a transaction to avoid partial migration if it fails.
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await dbContext.Database.MigrateAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    private static async Task SeedDataAsync(PodcastDbContext dbContext, CancellationToken cancellationToken)
    {
        var podcasts = new List<Podcast>
        {
            new(Guid.NewGuid(), "Unhandled Exception podcast"),
            new(Guid.NewGuid(), "Keep Coding Podcast"),
            new(Guid.NewGuid(), "Developer Weekly Podcast"),
            new(Guid.NewGuid(), "The Stack Overflow Podcast"),
            new(Guid.NewGuid(), "The Hanselminutes Podcast"),
            new(Guid.NewGuid(), "The .NET Rocks Podcast"),
            new(Guid.NewGuid(), "The Azure Podcast"),
            new(Guid.NewGuid(), "The AWS Podcast"),
            new(Guid.NewGuid(), "The Rabbit Hole Podcast"),
            new(Guid.NewGuid(), "The .NET Core Podcast"),
        };

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            if (!await dbContext.Podcasts.AnyAsync(cancellationToken))
            {
                // Seed the database
                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                await dbContext.Podcasts.AddRangeAsync(podcasts, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
        });
    }
}