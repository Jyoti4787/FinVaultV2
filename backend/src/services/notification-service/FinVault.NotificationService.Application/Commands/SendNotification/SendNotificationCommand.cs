// ==================================================================
// FILE : SendNotificationCommand.cs
// LAYER: Application (Commands)
// PATH : notification-service/FinVault.NotificationService.Application/Commands/SendNotification/
//
// WHAT IS THIS?
// A request to "Queue" a new alert for a user.
// This is used INTERNALLY by the consumers (listeners).
// ==================================================================

using System;
using MediatR;

namespace FinVault.NotificationService.Application.Commands.SendNotification;

public record SendNotificationCommand(
    Guid UserId,
    string Message,
    string Type,
    string? ActionUrl = null
) : IRequest<Guid>; // Returns the ID of the created notification
