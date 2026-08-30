using NotificationService.Services;
using NotificationService.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IEmailService, ConsoleEmailService>();
builder.Services.AddHostedService<RabbitMqConsumerWorker>();

var host = builder.Build();
host.Run();