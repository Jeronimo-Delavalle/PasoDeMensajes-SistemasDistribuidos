namespace OrderService.Services.Implementations;

using System.Text;
using System.Text.Json;
using OrderService.Services;
using RabbitMQ.Client;

public class RabbitMqPublisherService : IRabbitMqPublisherService
{
    private readonly string _hostname;
    private const string QueueName = "order_notifications";

    public RabbitMqPublisherService(IConfiguration configuration)
    {
        _hostname = configuration["RABBITMQ_HOST"] ?? "localhost";
    }

    public async Task PublishOrderCreated(string orderId, string customerEmail, string productId, int quantity)
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostname,
            RequestedConnectionTimeout = TimeSpan.FromSeconds(5)
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var message = new
        {
            Event = "ORDER_CREATED",
            OrderId = orderId,
            CustomerEmail = customerEmail,
            ProductId = productId,
            Quantity = quantity,
            Timestamp = DateTime.UtcNow
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent
        };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: QueueName,
            mandatory: false,
            basicProperties: properties,
            body: body);

        Console.WriteLine($"[RabbitMQ] Evento 'ORDER_CREATED' publicado para pedido: {orderId}");
    }
}
