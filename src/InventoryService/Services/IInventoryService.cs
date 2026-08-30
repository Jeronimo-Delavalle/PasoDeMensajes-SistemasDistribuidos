namespace InventoryService.Services;

using InventoryService.Models;

public interface IInventoryService
{
    InventoryStockResponse? CheckStock(string productId);
}