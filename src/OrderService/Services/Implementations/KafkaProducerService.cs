namespace OrderService.Services.Implementations;

using System.Text.Json;
using Confluent.Kafka;
using OrderService.Services;

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private const string TopicName = "order-events";

    public KafkaProducerService(IConfiguration config)
    {
        var bootstrapServers = config["KAFKA_BOOTSTRAP_SERVERS"] ?? "localhost:9092";
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All,
            // Falla rápido si Kafka no está disponible, para no bloquear la API.
            MessageTimeoutMs = 5000,
            SocketTimeoutMs = 5000
        };
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublishLifecycleEventAsync(string eventType, string orderId, object payload)
    {
        var messageValue = JsonSerializer.Serialize(new
        {
            EventType = eventType,
            OrderId = orderId,
            Data = payload,
            Timestamp = DateTime.UtcNow
        });

        await _producer.ProduceAsync(TopicName, new Message<string, string>
        {
            Key = orderId,
            Value = messageValue
        });

        Console.WriteLine($"[Kafka] Evento '{eventType}' emitido para pedido '{orderId}'");
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
