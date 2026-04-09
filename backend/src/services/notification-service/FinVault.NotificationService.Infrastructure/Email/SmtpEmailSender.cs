// ==================================================================
// FILE : SmtpEmailSender.cs
// LAYER: Infrastructure (Email)
// WHAT IS THIS?
// Real email sender using MailKit + Gmail SMTP.
// Reads config from appsettings.json → "Email" section.
//
// Features:
//   ✅ Structured logging — tracks sent/failed emails
//   ✅ Retry is handled at the MassTransit consumer level (5 retries)
//   ✅ Throws on failure so MassTransit retry kicks in
// ==================================================================

using FinVault.NotificationService.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace FinVault.NotificationService.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Sending email — To: {ToEmail}, Subject: {Subject}",
            toEmail, subject);

        try
        {
            // ── BUILD THE EMAIL MESSAGE ────────────────────────────
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                _config["Email:FromName"]  ?? "FinVault",
                _config["Email:FromEmail"] ?? throw new InvalidOperationException("Email:FromEmail not configured")));

            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = builder.ToMessageBody();

            // ── CONNECT AND SEND VIA SMTP ──────────────────────────
            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _config["Email:SmtpHost"] ?? "smtp.gmail.com",
                int.Parse(_config["Email:SmtpPort"] ?? "587"),
                SecureSocketOptions.StartTls,
                ct);

            await smtp.AuthenticateAsync(
                _config["Email:Username"],
                _config["Email:Password"],
                ct);

            await smtp.SendAsync(message, ct);
            await smtp.DisconnectAsync(true, ct);

            _logger.LogInformation(
                "Email sent successfully — To: {ToEmail}, Subject: {Subject}",
                toEmail, subject);
        }
        catch (Exception ex)
        {
            // Log the failure with full details — MassTransit retry will re-invoke this
            _logger.LogError(ex,
                "Email send FAILED — To: {ToEmail}, Subject: {Subject}, Error: {Error}",
                toEmail, subject, ex.Message);

            // Re-throw so MassTransit retry mechanism kicks in
            throw;
        }
    }
}
