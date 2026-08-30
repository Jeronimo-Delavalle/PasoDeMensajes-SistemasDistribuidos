namespace OrderService.Models
{
    public record OrderRequest(string OrderId, string ProductId, int Quantity, string CustomerEmail);

    public record OrderResponse(string OrderId, string Status, string Message);

    public record InventoryCheckDto(string ProductId, int Stock, bool IsAvailable);
    public record OrderStatusUpdateRequest(string Status);
}
