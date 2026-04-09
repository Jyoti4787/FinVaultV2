using FinVault.IdentityService.Domain.Events;
using MassTransit;
using MediatR;

namespace FinVault.IdentityService.Infrastructure.Messaging.Publishers;

public class PasswordResetRequestedMessage
{
    public string EventName     { get; set; } = string.Empty;
    public string SourceService { get; set; } = string.Empty;
    public Guid UserId          { get; set; }
    public string Email         { get; set; } = string.Empty;
    public string OtpCode       { get; set; } = string.Empty;
    public Guid CorrelationId   { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}

public class PasswordResetRequestedPublisher
    : INotificationHandler<PasswordResetRequestedDomainEvent>
{
    private readonly IPublishEndpoint _bus;

    public PasswordResetRequestedPublisher(IPublishEndpoint bus)
        => _bus = bus;

    public async Task Handle(
        PasswordResetRequestedDomainEvent evt,
        CancellationToken ct)
    {
        await _bus.Publish(new PasswordResetRequestedMessage
        {
            EventName     = "PasswordResetRequested",
            SourceService = "identity-service",
            UserId        = evt.UserId,
            Email         = evt.Email,
            OtpCode       = evt.OtpCode,
            CorrelationId = evt.CorrelationId,
            OccurredAt    = DateTimeOffset.UtcNow
        }, ct);
    }
}
