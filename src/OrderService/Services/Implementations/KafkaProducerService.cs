namespace OrdersService.Services.Implementations;

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
            Acks = Acks.All
        };
        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublishLifecycleEventAsync(string eventType, string orderId, object payload)
    {
        try
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

            Console.WriteLine($"[Kafka] 🚀 Evento '{eventType}' emitido para pedido '{orderId}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Kafka Error] {ex.Message}");
        }
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}