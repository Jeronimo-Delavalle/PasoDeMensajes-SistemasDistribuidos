namespace OrderService.Services
{
    public interface IRabbitMqPublisherService
    {
        Task PublishOrderCreated(string orderId, string customerEmail, string productId, int quantity);
    }
}
