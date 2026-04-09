// ==================================================================
// FILE : PaymentCompletedConsumer.cs
// LAYER: Infrastructure (Messaging → Consumers)
// WHAT IS THIS?
// Listens for PaymentCompletedMessage published by the Payment Saga
// when a payment fully completes (billing confirmed).
// 1. Saves a notification to SQL Server for the user.
// 2. Sends a payment receipt email.
// ==================================================================

using FinVault.NotificationService.Application.Commands.SendNotification;
using FinVault.NotificationService.Application.Interfaces;
using FinVault.Shared.Contracts.Messages;
using MassTransit;
using MediatR;

namespace FinVault.NotificationService.Infrastructure.Messaging.Consumers;

public class PaymentCompletedConsumer : IConsumer<PaymentCompletedMessage>
{
    private readonly IMediator    _mediator;
    private readonly IEmailSender _emailSender;

    public PaymentCompletedConsumer(IMediator mediator, IEmailSender emailSender)
    {
        _mediator    = mediator;
        _emailSender = emailSender;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedMessage> context)
    {
        var msg = context.Message;

        // 1. Save in-app notification
        await _mediator.Send(new SendNotificationCommand(
            msg.UserId,
            $"Payment of ₹{msg.Amount:N2} was processed successfully.",
            "Payment"
        ));

        // 2. Send email receipt (only if email is available on the message)
        if (!string.IsNullOrWhiteSpace(msg.UserEmail))
        {
            var subject = "✅ FinVault Payment Receipt";
            var body = $"""
                <!DOCTYPE html>
                <html>
                <body style="font-family:'Segoe UI',Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;">
                  <div style="max-width:480px;margin:auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,0.1);">
                    <div style="background:linear-gradient(135deg,#11998e 0%,#38ef7d 100%);padding:32px 24px;text-align:center;">
                      <h1 style="color:#fff;margin:0;font-size:22px;">✅ Payment Successful</h1>
                      <p style="color:rgba(255,255,255,0.85);margin:8px 0 0;font-size:14px;">FinVault</p>
                    </div>
                    <div style="padding:32px 24px;">
                      <p style="color:#444;font-size:15px;">Your payment has been processed successfully.</p>
                      <table style="width:100%;border-collapse:collapse;margin:20px 0;">
                        <tr style="background:#f8fff8;">
                          <td style="padding:12px;border:1px solid #e0f0e0;color:#555;font-size:14px;">Amount</td>
                          <td style="padding:12px;border:1px solid #e0f0e0;color:#2a7a2a;font-size:18px;font-weight:800;">₹{msg.Amount:N2}</td>
                        </tr>
                        <tr>
                          <td style="padding:12px;border:1px solid #eee;color:#555;font-size:14px;">Date</td>
                          <td style="padding:12px;border:1px solid #eee;color:#333;font-size:14px;">{DateTime.UtcNow:dd MMM yyyy, HH:mm} UTC</td>
                        </tr>
                      </table>
                      <p style="color:#888;font-size:13px;">If you did not authorize this payment, contact support immediately.</p>
                    </div>
                    <div style="background:#f8f9ff;padding:16px 24px;text-align:center;border-top:1px solid #eee;">
                      <p style="color:#aaa;font-size:12px;margin:0;">© 2025 FinVault · Secure Banking Platform</p>
                    </div>
                  </div>
                </body>
                </html>
                """;

            await _emailSender.SendAsync(msg.UserEmail, subject, body, context.CancellationToken);
        }
    }
}
