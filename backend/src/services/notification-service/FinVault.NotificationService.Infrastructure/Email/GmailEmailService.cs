using FinVault.NotificationService.Application.Interfaces;
using FinVault.NotificationService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace FinVault.NotificationService.Infrastructure.Email;

public class GmailEmailService : IEmailService, IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<GmailEmailService> _logger;

    public GmailEmailService(IConfiguration config, ILogger<GmailEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        var emailMessage = new MimeMessage();
        var senderName = _config["Email:FromName"] ?? "FinVault";
        var senderEmail = _config["Email:FromEmail"];
        
        if (string.IsNullOrEmpty(senderEmail))
        {
            _logger.LogWarning("Email sending skipped. Email:FromEmail not configured.");
            return;
        }

        emailMessage.From.Add(new MailboxAddress(senderName, senderEmail));
        emailMessage.To.Add(new MailboxAddress("", toEmail));
        emailMessage.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        emailMessage.Body = bodyBuilder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            var host = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var port = int.TryParse(_config["Email:SmtpPort"], out int p) ? p : 587;
            
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, ct);
            
            var password = _config["Email:Password"];
            if (!string.IsNullOrEmpty(password))
            {
                await client.AuthenticateAsync(senderEmail, password, ct);
            }
            
            await client.SendAsync(emailMessage, ct);
            await client.DisconnectAsync(true, ct);
            
            _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
            throw;
        }
    }

    public async Task SendOtpEmailAsync(string toEmail, string otpCode, string purpose, CancellationToken ct = default)
    {
        var subject = $"Your FinVault OTP Code";
        var action = purpose.ToLower() switch
        {
            "login" => "log in to your account",
            "passwordreset" => "reset your password",
            "payment" => "confirm your payment",
            "cardreveal" => "reveal your card details",
            "signupverification" => "complete your registration",
            _ => "verify your requested action"
        };

        var html = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; text-align: center; color: #333;'>
                <h2>FinVault Security Verification</h2>
                <p>You requested to <b>{action}</b>.</p>
                <div style='margin: 20px auto; padding: 15px; border-radius: 8px; background-color: #f4f4f4; width: max-content;'>
                    <h1 style='margin: 0; letter-spacing: 5px; color: #1a73e8;'>{otpCode}</h1>
                </div>
                <p>This code expires in 5 minutes.</p>
                <p style='color: #888; font-size: 12px; margin-top: 30px;'>If you didn't request this, please ignore this email.</p>
            </div>";

        await SendAsync(toEmail, subject, html, ct);
    }

    public async Task SendPaymentConfirmationAsync(string toEmail, decimal amount, CancellationToken ct = default)
    {
        var subject = "Payment Processed Successfully";
        var html = $"<h2>Payment Confirmation</h2><p>Your payment of ${amount:F2} has been successfully processed.</p>";
        await SendAsync(toEmail, subject, html, ct);
    }

    public async Task SendCardAddedAsync(string toEmail, string maskedCard, CancellationToken ct = default)
    {
        var subject = "New Card Added to FinVault";
        var html = $"<h2>Card Added</h2><p>The card {maskedCard} has been successfully added to your account.</p>";
        await SendAsync(toEmail, subject, html, ct);
    }
}
