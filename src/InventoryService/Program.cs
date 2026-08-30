using InventoryService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IInventoryService, InMemoryInventoryService>();

var app = builder.Build();

app.MapControllers();
app.Run();