// ==================================================================
// FILE : SendNotificationCommandHandler.cs
// LAYER: Application (Commands)
// PATH : notification-service/FinVault.NotificationService.Application/Commands/SendNotification/
//
// WHAT IS THIS?
// The worker that creates the Notification entity and saves it.
// ==================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using FinVault.NotificationService.Domain.Entities;
using FinVault.NotificationService.Domain.Interfaces;
using MediatR;

namespace FinVault.NotificationService.Application.Commands.SendNotification;

public class SendNotificationCommandHandler 
    : IRequestHandler<SendNotificationCommand, Guid>
{
    private readonly INotificationRepository _repository;

    public SendNotificationCommandHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(
        SendNotificationCommand request, 
        CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Id        = Guid.NewGuid(),
            UserId    = request.UserId,
            Message   = request.Message,
            Type      = request.Type,
            ActionUrl = request.ActionUrl,
            SentDate  = DateTime.UtcNow,
            IsRead    = false
        };

        await _repository.AddAsync(notification);
        return notification.Id;
    }
}
