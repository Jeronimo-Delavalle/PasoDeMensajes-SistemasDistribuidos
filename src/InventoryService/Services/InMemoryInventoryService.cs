namespace InventoryService.Services;

using InventoryService.Models;

public class InMemoryInventoryService : IInventoryService
{
    // Simulación de base de datos de stock
    private readonly Dictionary<string, int> _stockDb = new()
    {
        ["prod-1"] = 10,
        ["prod-2"] = 0,   // Sin stock
        ["prod-3"] = 5
    };

    public InventoryStockResponse? CheckStock(string productId)
    {
        if (!_stockDb.TryGetValue(productId, out int stock))
            return null;

        return new InventoryStockResponse(productId, stock, stock > 0);
    }
}