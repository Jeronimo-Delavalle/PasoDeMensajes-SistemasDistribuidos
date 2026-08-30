
namespace OrderService.Services;
    using OrderService.Models;

    public interface IInventoryClientService
    {
        Task<InventoryCheckDto?> VerifyStockAsync(string productId);
    }


