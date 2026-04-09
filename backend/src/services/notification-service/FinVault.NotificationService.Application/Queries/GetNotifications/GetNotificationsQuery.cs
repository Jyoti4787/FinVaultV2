// ==================================================================
// FILE : GetNotificationsQuery.cs
// LAYER: Application (Queries)
// PATH : notification-service/FinVault.NotificationService.Application/Queries/GetNotifications/
//
// WHAT IS THIS?
// "Show me my alerts!" 
// Returns a list of notifications for the logged-in user.
// ==================================================================

using System;
using System.Collections.Generic;
using MediatR;

namespace FinVault.NotificationService.Application.Queries.GetNotifications;

public record GetNotificationsQuery(
    Guid UserId
) : IRequest<IEnumerable<NotificationDto>>;

public record NotificationDto(
    Guid Id,
    string Message,
    string Type,
    DateTime Date,
    bool IsRead,
    string? ActionUrl
);
