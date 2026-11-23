namespace OrderService.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using OrderService.Models;
    using OrderService.Services;

    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IInventoryClientService _inventoryClient;
        private readonly IRabbitMqPublisherService _rabbitMqPublisher;
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IInventoryClientService inventoryClient,
            IRabbitMqPublisherService rabbitMqPublisherService,
            IKafkaProducerService kafkaProducer,
            ILogger<OrdersController> logger)
        {
            _inventoryClient = inventoryClient;
            _rabbitMqPublisher = rabbitMqPublisherService;
            _kafkaProducer = kafkaProducer;
            _logger = logger;
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
            catch (ProductNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(503, new { message = ex.Message });
            }

            if (inventory is null || !inventory.IsAvailable || inventory.Stock < request.Quantity)
            {
                return BadRequest(new
                {
                    message = $"Stock insuficiente para el producto '{request.ProductId}'."
                });
            }

            // 2. Notificación asíncrona por RabbitMQ (Ejercicio 2).
            // Si RabbitMQ no está disponible, no confirmamos el pedido: la notificación es parte del flujo.
            try
            {
                await _rabbitMqPublisher.PublishOrderCreated(
                    request.OrderId,
                    request.CustomerEmail,
                    request.ProductId,
                    request.Quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo publicar ORDER_CREATED en RabbitMQ para {OrderId}", request.OrderId);
                return StatusCode(503, new
                {
                    message = "No se pudo publicar la notificación del pedido. Intentá nuevamente."
                });
            }

            // 3. Evento de ciclo de vida en Kafka (Ejercicio 3).
            // La analítica no bloquea la confirmación: si Kafka falla, se avisa en la respuesta.
            string? warning = null;
            try
            {
                await _kafkaProducer.PublishLifecycleEventAsync(
                    "ORDER_CREATED",
                    request.OrderId,
                    new
                    {
                        request.ProductId,
                        request.Quantity,
                        request.CustomerEmail,
                        Status = "CREATED"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo publicar ORDER_CREATED en Kafka para {OrderId}", request.OrderId);
                warning = "El pedido se confirmó y la notificación se encoló, pero no se pudo publicar el evento de analítica.";
            }

            return Ok(new OrderResponse(
                request.OrderId,
                "CONFIRMED",
                "Pedido validado y confirmado síncronamente vía REST.",
                warning));
        }

        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateStatus(string orderId, [FromBody] OrderStatusUpdateRequest request)
        {
            var status = request.Status.Trim();
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest(new { message = "El estado no puede estar vacío." });

            var eventType = $"ORDER_{status.ToUpperInvariant()}";

            try
            {
                await _kafkaProducer.PublishLifecycleEventAsync(
                    eventType,
                    orderId,
                    new { Status = status });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo publicar {EventType} en Kafka para {OrderId}", eventType, orderId);
                return StatusCode(503, new { message = "No se pudo publicar el evento en Kafka. Intentá nuevamente." });
            }

            return Ok(new
            {
                orderId,
                newStatus = status,
                message = "Estado publicado en Kafka."
            });
        }
    }
}
