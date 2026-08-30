namespace NotificationService.Workers;

using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NotificationService.Services;

public class RabbitMqConsumerWorker : BackgroundService
{
    private readonly ILogger<RabbitMqConsumerWorker> _logger;
    private readonly IEmailService _emailService;
    private readonly string _hostname;
    private const string QueueName = "order_notifications";

    public RabbitMqConsumerWorker(ILogger<RabbitMqConsumerWorker> logger, IEmailService emailService, IConfiguration config)
    {
        _logger = logger;
        _emailService = emailService;
        _hostname = config["RABBITMQ_HOST"] ?? "localhost";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[RabbitMQ Worker] Conectando a {Host}...", _hostname);
        IConnection? connection = null;
        IModel? channel = null;

        while (!stoppingToken.IsCancellationRequested && connection == null)
        {
            try
            {
                var factory = new ConnectionFactory { HostName = _hostname, DispatchConsumersAsync = true };
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
            }
            catch
            {
                _logger.LogWarning("[RabbitMQ Worker] Broker no disponible. Reintentando en 3s...");
                await Task.Delay(3000, stoppingToken);
            }
        }

        if (channel == null) return;

        channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        channel.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            _logger.LogInformation("\n[📨 RabbitMQ Recibido] {Message}", message);

            using var doc = JsonDocument.Parse(message);
            var email = doc.RootElement.GetProperty("CustomerEmail").GetString() ?? "";
            var orderId = doc.RootElement.GetProperty("OrderId").GetString() ?? "";

            await _emailService.SendOrderConfirmationEmailAsync(email, orderId);

            // ACK manual
            channel.BasicAck(ea.DeliveryTag, multiple: false);
        };

        channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
            await Task.Delay(1000, stoppingToken);

        channel.Close();
        connection?.Close();
    }
}