using Entities;
using MigrationService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<PodcastDbContext>("podcasts");

var host = builder.Build();
host.Run();
