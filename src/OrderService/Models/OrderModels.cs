namespace OrderService.Models
{
    using System.ComponentModel.DataAnnotations;

    public record OrderRequest(
        [Required(ErrorMessage = "El orderId es obligatorio."), MinLength(1)]
        string OrderId,

        [Required(ErrorMessage = "El productId es obligatorio."), MinLength(1)]
        string ProductId,

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
        int Quantity,

        [Required(ErrorMessage = "El email del cliente es obligatorio."), EmailAddress(ErrorMessage = "El email del cliente no es válido.")]
        string CustomerEmail);

    public record OrderResponse(string OrderId, string Status, string Message, string? Warning = null);

    public record InventoryCheckDto(string ProductId, int Stock, bool IsAvailable);

    public record OrderStatusUpdateRequest(
        [Required(ErrorMessage = "El estado es obligatorio."), MinLength(1)]
        string Status);
}
