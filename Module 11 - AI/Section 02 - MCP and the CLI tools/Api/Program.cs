using System.Diagnostics.Metrics;
using Api;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();

builder.AddServiceDefaults();

builder.Services.AddHttpClient<RatingServiceHttpClient>(x => x.BaseAddress = new Uri("https+http://ratingservice"));

builder.Services.AddSingleton(TracerProvider.Default.GetTracer(builder.Environment.ApplicationName));

builder.AddSqlServerDbContext<PodcastDbContext>(connectionName: "podcasts",
x => x.DisableTracing = true);

builder.Services.AddOutputCache();

builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder.AddSqlClientInstrumentation();
});

var app = builder.Build();

app.UseCors(x => x.AllowAnyOrigin());
app.UseOutputCache();

app.MapGet("/podcasts", async (PodcastDbContext db, RatingServiceHttpClient ratingServiceHttpClient, Tracer tracer) =>
{
    var all = await db.Podcasts
        .OrderBy(x => x.Title)
        .ToListAsync();

    using (var span = tracer.StartActiveSpan("Get all podcast ratings"))
    {
        span.SetAttribute("Count", all.Count);

        var ratings = await ratingServiceHttpClient.GetRatings(all.Select(p => p.Title));

        return all.Select(podcast => new PodcastApiModel(
            podcast.Title,
            ratings.GetValueOrDefault(podcast.Title, 0)
        )).ToArray();
    }
}).CacheOutput(p => p.Expire(TimeSpan.FromSeconds(10)));

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

record PodcastApiModel(string PodcastName, int Rating);

public partial class Program;
