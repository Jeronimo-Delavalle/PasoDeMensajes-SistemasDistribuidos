namespace OrderService.Services;

public interface IKafkaProducerService
{
    Task PublishLifecycleEventAsync(string eventType, string orderId, object payload);
}