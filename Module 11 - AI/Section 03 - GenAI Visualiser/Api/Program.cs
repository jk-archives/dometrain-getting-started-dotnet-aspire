using System.Diagnostics.Metrics;
using Api;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();

builder.AddServiceDefaults();

builder.AddOpenAIClient("chat", settings =>
    {
        settings.EnableSensitiveTelemetryData = true;
    })
    .AddChatClient()
    .UseFunctionInvocation();

builder.Services.AddHttpClient<RatingServiceHttpClient>(x => x.BaseAddress = new Uri("https+http://ratingservice"));

builder.Services.AddSingleton(TracerProvider.Default.GetTracer(builder.Environment.ApplicationName));

builder.Services.AddSingleton<AiUtility>();
builder.Services.Configure<ConfigOptions>(builder.Configuration);

builder.AddSqlServerDbContext<PodcastDbContext>(connectionName: "podcasts",
x => x.DisableTracing = true);

builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder.AddSqlClientInstrumentation();
});

var app = builder.Build();

app.UseCors(x => x.AllowAnyOrigin());

app.MapGet("/podcasts", async (
    PodcastDbContext db,
    RatingServiceHttpClient ratingServiceHttpClient,
    AiUtility aiUtility,
    Tracer tracer) =>
{
    var all = await db.Podcasts
        .OrderBy(x => x.Title)
        .ToListAsync();

    var withRatings = new List<PodcastApiModel>();

    using (var span = tracer.StartActiveSpan("Get all podcast ratings"))
    {
        span.SetAttribute("Count", all.Count);

        foreach (var podcast in all)
        {
            var desc = await aiUtility.GetPodcastDescription(podcast.Title);

            withRatings.Add(new PodcastApiModel(
                podcast.Title,
                await ratingServiceHttpClient.GetRating(podcast.Title),
                desc
            ));
        }
    }

    return withRatings;
});

var demoMeter = new Meter("Dometrain.AspireCourse", "1.0");
var ratingsSubmittedCounter = demoMeter.CreateCounter<int>("ratings_submitted");

app.MapPost("/rating", async (RatingServiceHttpClient ratingServiceHttpClient, [FromBody] PodcastApiModel podcast) =>
{
    await ratingServiceHttpClient.SubmitRating(podcast.PodcastName, podcast.Rating);

    ratingsSubmittedCounter.Add(1);

    Tracer.CurrentSpan.SetAttribute("PodcastName", podcast.PodcastName);
    Tracer.CurrentSpan.SetAttribute("Rating", podcast.Rating);
});

app.MapDefaultEndpoints();

app.Run();

record PodcastApiModel(string PodcastName, int Rating, string Description);

public partial class Program;
