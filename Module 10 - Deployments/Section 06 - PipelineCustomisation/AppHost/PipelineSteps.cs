using Aspire.Hosting.Pipelines;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.DependencyInjection;

namespace AppHost;

#pragma warning disable ASPIREPIPELINES001
#pragma warning disable ASPIREPIPELINES004

public static class PipelineSteps
{
    public static void AddCustomPipelineSteps(IDistributedApplicationBuilder builder)
    {
        const string imageTag = "2";
        const string password = "fdsafsd23423#123";
        var services = new[] { "api", "frontend", "ratingservice", "migration" };

        builder.Pipeline.AddStep(
            "tag-images",
            async stepContext =>
            {
                var ct = stepContext.CancellationToken;

                foreach (var service in services)
                {
                    var result = await Cli.Wrap("docker")
                        .WithArguments(["tag", $"{service}:latest", $"{service}:{imageTag}"])
                        .WithValidation(CommandResultValidation.None)
                        .ExecuteBufferedAsync(ct);

                    if (result.ExitCode != 0)
                    {
                        await stepContext.ReportingStep
                            .FailAsync($"Failed to tag {service}: {result.StandardError}", ct);
                        return;
                    }
                }

                await stepContext.ReportingStep
                    .SucceedAsync($"Tagged {services.Length} images as '{imageTag}'", ct);
            },
            dependsOn: WellKnownPipelineSteps.Build,
            requiredBy: "helm-install"
        );

        builder.Pipeline.AddStep(
            "helm-install",
            async stepContext =>
            {
                var ct = stepContext.CancellationToken;

                var outputService = stepContext.PipelineContext.Services.GetRequiredService<IPipelineOutputService>();
                var chartPath = outputService.GetOutputDirectory();

                var result = await Cli.Wrap("helm")
                    .WithArguments([
                        "upgrade", "--install", "--wait",
                        "--namespace", "podcast", "--create-namespace",
                        // Image tags
                        "--set", $"parameters.api.api_image=api:{imageTag}",
                        "--set", $"parameters.frontend.frontend_image=frontend:{imageTag}",
                        "--set", $"parameters.ratingservice.ratingservice_image=ratingservice:{imageTag}",
                        "--set", $"parameters.migration.migration_image=migration:{imageTag}",
                        // Secrets
                        "--set", $"secrets.ratingservice.cache_password={password}",
                        "--set", $"secrets.migration.password={password}",
                        "--set", $"secrets.api.password={password}",
                        "--set", $"secrets.server.password={password}",
                        "--set", $"secrets.server.MSSQL_SA_PASSWORD={password}",
                        "--set", $"secrets.cache.REDIS_PASSWORD={password}",
                        "--set", $"secrets.cache.cache_password={password}",
                        // Release name and chart path
                        "podcast", chartPath
                    ])
                    .WithValidation(CommandResultValidation.None)
                    .ExecuteBufferedAsync(ct);

                if (result.ExitCode != 0)
                {
                    await stepContext.ReportingStep
                        .FailAsync($"Helm install failed: {result.StandardError}", ct);
                    return;
                }

                await stepContext.ReportingStep
                    .SucceedAsync("Deployed to namespace 'podcast'", ct);
            },
            dependsOn: "tag-images",
            requiredBy: WellKnownPipelineSteps.Deploy
        );
    }
}
