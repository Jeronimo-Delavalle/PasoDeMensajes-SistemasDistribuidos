namespace InventoryService.Controllers;

using Microsoft.AspNetCore.Mvc;
using InventoryService.Services;

[ApiController]
[Route("[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("{productId}")]
    public IActionResult GetStock(string productId)
    {
        var result = _inventoryService.CheckStock(productId);
        if (result == null)
            return NotFound(new { message = $"Producto '{productId}' no encontrado en inventario." });

        return Ok(result);
    }
}