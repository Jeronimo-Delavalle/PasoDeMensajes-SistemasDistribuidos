using AnalyticsService.Services;
using AnalyticsService.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IAnalyticsMetricsService, InMemoryAnalyticsMetricsService>();
builder.Services.AddHostedService<KafkaAnalyticsWorker>();

var host = builder.Build();
host.Run();