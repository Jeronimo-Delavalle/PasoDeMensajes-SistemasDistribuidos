namespace NotificationService.Services;

public interface IEmailService
{
    Task SendOrderConfirmationEmailAsync(string email, string orderId);
}
