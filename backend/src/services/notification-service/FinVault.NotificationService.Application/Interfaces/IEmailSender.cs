// ==================================================================
// FILE : IEmailSender.cs
// LAYER: Application (Interfaces)
// WHAT IS THIS?
// The contract that any email-sending implementation must fulfill.
// The Application layer only knows about this interface.
// The actual SMTP/SendGrid implementation lives in Infrastructure.
// ==================================================================

namespace FinVault.NotificationService.Application.Interfaces;

public interface IEmailSender
{
    /// <summary>
    /// Sends an HTML email to the given address.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="subject">Email subject line</param>
    /// <param name="htmlBody">HTML content of the email</param>
    /// <param name="ct">Cancellation token</param>
    Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken ct = default);
}
