namespace NotificationService.Services;

public interface IEmailService
{
    Task SendOrderConfirmationEmailAsync(string email, string orderId);

    Task SendOrderStatusNotificationAsync(string orderId, string status);
}
