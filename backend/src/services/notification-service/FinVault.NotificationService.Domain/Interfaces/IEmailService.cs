namespace FinVault.NotificationService.Domain.Interfaces;

public interface IEmailService
{
    Task SendOtpEmailAsync(string toEmail, string otpCode, string purpose, CancellationToken ct = default);
    Task SendPaymentConfirmationAsync(string toEmail, decimal amount, CancellationToken ct = default);
    Task SendCardAddedAsync(string toEmail, string maskedCard, CancellationToken ct = default);
}
