namespace NotificationService.Workers;

using System.Text.Json;
using Confluent.Kafka;
using NotificationService.Services;

/// <summary>
/// Consume los eventos del ciclo de vida del pedido desde Kafka.
/// RabbitMQ sigue usándose para el correo de "pedido creado" (Ejercicio 2);
/// este worker atiende los cambios de estado (confirmado, enviado, etc.).
/// </summary>
public class KafkaNotificationWorker : BackgroundService
{
    private readonly ILogger<KafkaNotificationWorker> _logger;
    private readonly IEmailService _emailService;
    private readonly string _bootstrapServers;

    public KafkaNotificationWorker(
        ILogger<KafkaNotificationWorker> logger,
        IEmailService emailService,
        IConfiguration config)
    {
        _logger = logger;
        _emailService = emailService;
        _bootstrapServers = config["KAFKA_BOOTSTRAP_SERVERS"] ?? "localhost:9092";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[Kafka Notification] Esperando broker en {Servers}...", _bootstrapServers);
        await Task.Delay(5000, stoppingToken);

        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = "notification-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe("order-events");

        _logger.LogInformation(
            "[*] NotificationService escuchando topic 'order-events' (Group: notification-group)");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromMilliseconds(500));
                if (result is null)
                    continue;

                using var doc = JsonDocument.Parse(result.Message.Value);
                var eventType = doc.RootElement.GetProperty("EventType").GetString() ?? "";
                var orderId = doc.RootElement.GetProperty("OrderId").GetString() ?? "";

                // ORDER_CREATED ya se notifica por RabbitMQ; evitamos enviar el correo dos veces.
                if (eventType == "ORDER_CREATED")
                {
                    _logger.LogInformation(
                        "[Kafka Notification] ORDER_CREATED de '{OrderId}' ya notificado por RabbitMQ.",
                        orderId);
                    continue;
                }

                await _emailService.SendOrderStatusNotificationAsync(orderId, eventType);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError("[Kafka Notification Error] {Reason}", ex.Error.Reason);
            }
            catch (JsonException ex)
            {
                _logger.LogError("[Kafka Notification Error] Mensaje inválido: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("[Kafka Notification Error] {Message}", ex.Message);
            }
        }

        consumer.Close();
    }
}
