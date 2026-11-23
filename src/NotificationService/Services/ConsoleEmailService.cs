namespace NotificationService.Services
{
    public class ConsoleEmailService : IEmailService
    {
        private readonly ILogger<ConsoleEmailService> _logger;

        public ConsoleEmailService(ILogger<ConsoleEmailService> logger) => _logger = logger;

        public async Task SendOrderConfirmationEmailAsync(string email, string orderId)
        {
            _logger.LogInformation(
                " -> [EmailService] Enviando confirmación de pedido '{OrderId}' a '{Email}'...",
                orderId,
                email);

            await Task.Delay(1000); // Simulación de I/O de red

            _logger.LogInformation(" -> [EmailService] Correo enviado exitosamente.");
        }

        public async Task SendOrderStatusNotificationAsync(string orderId, string status)
        {
            _logger.LogInformation(
                " -> [EmailService] Notificando cambio de estado '{Status}' del pedido '{OrderId}'...",
                status,
                orderId);

            await Task.Delay(500); // Simulación de I/O de red

            _logger.LogInformation(" -> [EmailService] Aviso de estado enviado exitosamente.");
        }
    }
}
