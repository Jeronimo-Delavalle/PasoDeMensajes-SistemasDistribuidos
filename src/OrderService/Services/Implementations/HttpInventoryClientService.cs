namespace OrderService.Services.Implementations;
 using OrderService.Models;
 using OrderService.Services;
 using System.Net.Http;
public class HttpInventoryClientService: IInventoryClientService
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
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<InventoryCheckDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"InventoryService no disponible: {ex.Message}", ex);
        }
    }
}

