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

        public OrdersController(IInventoryClientService inventoryClient)
        {
            _inventoryClient = inventoryClient;
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

            return Ok(new OrderResponse(request.OrderId, "CONFIRMED", "Pedido validado y confirmado síncronamente vía REST."));
        }
    }
}
