using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Services
{
    public class ConsoleEmailService : IEmailService
    {
        private readonly ILogger<ConsoleEmailService> _logger;

        public ConsoleEmailService(ILogger<ConsoleEmailService> logger) => _logger = logger;

        public async Task SendOrderConfirmationEmailAsync(string email, string orderId)
        {
            _logger.LogInformation(" -> [EmailService] Enviando confirmación de pedido '{OrderId}' a '{Email}'...", orderId, email);
            await Task.Delay(1000); // Simulación de I/O de red
            _logger.LogInformation(" -> [EmailService] ✅ Correo enviado exitosamente.");
        }
    }
}
