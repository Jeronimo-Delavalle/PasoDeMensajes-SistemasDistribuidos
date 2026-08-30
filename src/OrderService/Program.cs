using OrderService.Services;
using OrderService.Services.Implementations;
using OrdersService.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Configuración de Typed HttpClient para InventoryService
var inventoryUrl = builder.Configuration["INVENTORY_SERVICE_URL"] ?? "http://localhost:5001";
builder.Services.AddHttpClient<IInventoryClientService, HttpInventoryClientService>(client =>
{
    client.BaseAddress = new Uri(inventoryUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddScoped<IRabbitMqPublisherService, RabbitMqPublisherService>();

builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

var app = builder.Build();

app.MapControllers();
app.Run();