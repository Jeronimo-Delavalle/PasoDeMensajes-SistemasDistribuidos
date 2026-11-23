namespace OrderService.Services.Implementations;

using System.Net;
using OrderService.Models;
using OrderService.Services;

public class HttpInventoryClientService : IInventoryClientService
{
    private readonly HttpClient _httpClient;

    public HttpInventoryClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<InventoryCheckDto?> VerifyStockAsync(string productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/inventory/{productId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new ProductNotFoundException($"Producto '{productId}' no encontrado en inventario.");

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"InventoryService respondió {(int)response.StatusCode} ({response.ReasonPhrase}).");

            var inventory = await response.Content.ReadFromJsonAsync<InventoryCheckDto>();
            if (inventory is null)
                throw new InvalidOperationException("InventoryService devolvió una respuesta vacía.");

            return inventory;
        }
        catch (ProductNotFoundException)
        {
            throw;
        }
        catch (TaskCanceledException ex)
        {
            throw new InvalidOperationException("InventoryService no respondió a tiempo.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"InventoryService no disponible: {ex.Message}", ex);
        }
    }
}
