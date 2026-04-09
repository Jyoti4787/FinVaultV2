using FinVault.IdentityService.Domain.Events;
using MassTransit;
using MediatR;

namespace FinVault.IdentityService.Infrastructure.Messaging.Publishers;

// Named message class — MassTransit requires this
// Cannot use anonymous objects like new { ... }
public class UserRegisteredMessage
{
    public string EventName     { get; set; } = string.Empty;
    public string SourceService { get; set; } = string.Empty;
    public Guid UserId          { get; set; }
    public string Email         { get; set; } = string.Empty;
    public string FirstName     { get; set; } = string.Empty;
    public string LastName      { get; set; } = string.Empty;
    public Guid CorrelationId   { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}

public class UserRegisteredPublisher
    : INotificationHandler<UserRegisteredDomainEvent>
{
    private readonly IPublishEndpoint _bus;

    public UserRegisteredPublisher(IPublishEndpoint bus)
        => _bus = bus;

    public async Task Handle(
        UserRegisteredDomainEvent evt,
        CancellationToken ct)
    {
        await _bus.Publish(new UserRegisteredMessage
        {
            EventName     = "UserRegistered",
            SourceService = "identity-service",
            UserId        = evt.UserId,
            Email         = evt.Email,
            FirstName     = evt.FirstName,
            LastName      = evt.LastName,
            CorrelationId = evt.CorrelationId,
            OccurredAt    = DateTimeOffset.UtcNow
        }, ct);
    }
}
