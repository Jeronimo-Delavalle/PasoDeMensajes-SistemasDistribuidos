namespace AnalyticsService.Workers;

using System.Text.Json;
using Confluent.Kafka;
using AnalyticsService.Services;

public class KafkaAnalyticsWorker : BackgroundService
{
    private readonly ILogger<KafkaAnalyticsWorker> _logger;
    private readonly IAnalyticsMetricsService _metricsService;
    private readonly string _bootstrapServers;

    public KafkaAnalyticsWorker(ILogger<KafkaAnalyticsWorker> logger, IAnalyticsMetricsService metricsService, IConfiguration config)
    {
        _logger = logger;
        _metricsService = metricsService;
        _bootstrapServers = config["KAFKA_BOOTSTRAP_SERVERS"] ?? "localhost:9092";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[Kafka Analytics] Esperando broker en {Servers}...", _bootstrapServers);
        await Task.Delay(5000, stoppingToken);

        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = "analytics-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe("order-events");

        _logger.LogInformation("[*] AnalyticsService escuchando topic 'order-events' (Group: analytics-group)");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromMilliseconds(500));
                if (result == null) continue;

                using var doc = JsonDocument.Parse(result.Message.Value);
                var eventType = doc.RootElement.GetProperty("EventType").GetString() ?? "";

                _logger.LogInformation("\n[Kafka Analytics Recibido] Evento: {EventType}", eventType);
                _metricsService.RegisterEvent(eventType);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Kafka Analytics Error] {Message}", ex.Message);
            }
        }

        consumer.Close();
    }
}