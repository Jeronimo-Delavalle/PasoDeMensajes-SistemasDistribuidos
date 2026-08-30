namespace OrderService.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using OrderService.Models;
    using OrderService.Services;
   


    [ApiController]
    [Route("[controller]")]
    public class OrdersController: ControllerBase
    {
        private readonly IInventoryClientService _inventoryClient;
        private readonly IRabbitMqPublisherService _rabbitMqPublisher;
        private readonly IKafkaProducerService _kafkaProducer;

        public OrdersController(IInventoryClientService inventoryClient, IRabbitMqPublisherService rabbitMqPublisherService, IKafkaProducerService kafkaProducer)
        {
            _inventoryClient = inventoryClient;
            _rabbitMqPublisher = rabbitMqPublisherService;
            _kafkaProducer = kafkaProducer;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            // 1. Consulta síncrona REST (Ejercicio 1)
            InventoryCheckDto? inventory;
            try
            {
                inventory = await _inventoryClient.VerifyStockAsync(request.ProductId);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }

            if (inventory == null || !inventory.IsAvailable || inventory.Stock < request.Quantity)
            {
                return BadRequest(new { message = $"Stock insuficiente para el producto '{request.ProductId}'." });

            }
            await _rabbitMqPublisher.PublishOrderCreated(request.OrderId, request.CustomerEmail, request.ProductId, request.Quantity);

            return Ok(new OrderResponse(request.OrderId, "CONFIRMED", "Pedido validado y confirmado síncronamente vía REST."));

           
        }
       
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateStatus(string orderId, [FromBody] OrderStatusUpdateRequest request)
        {
            await _kafkaProducer.PublishLifecycleEventAsync($"ORDER_{request.Status.ToUpper()}", orderId, new { Status = request.Status });
            return Ok(new { orderId, newStatus = request.Status, message = "Estado publicado en Kafka." });
        }
    }
}
