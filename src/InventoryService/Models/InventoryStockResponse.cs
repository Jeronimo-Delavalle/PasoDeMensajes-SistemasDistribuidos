namespace InventoryService.Models;

public record InventoryStockResponse(
    string ProductId, 
    int Stock, 
    bool IsAvailable
    );