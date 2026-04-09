using FinVault.NotificationService.Domain.Interfaces;
using FinVault.Shared.Contracts.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FinVault.NotificationService.Infrastructure.Messaging.Consumers;

public class OtpRequestedConsumer : IConsumer<OtpRequestedMessage>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OtpRequestedConsumer> _logger;

    public OtpRequestedConsumer(IEmailService emailService, ILogger<OtpRequestedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OtpRequestedMessage> context)
    {
        var msg = context.Message;
        _logger.LogInformation("Received OTP Request for {Email} (Purpose: {Purpose})", msg.Email, msg.Purpose);

        await _emailService.SendOtpEmailAsync(msg.Email, msg.OtpCode, msg.Purpose, context.CancellationToken);
    }
}
