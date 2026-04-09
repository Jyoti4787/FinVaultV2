// ==================================================================
// FILE : GetNotificationsQueryHandler.cs
// LAYER: Application (Queries)
// PATH : notification-service/FinVault.NotificationService.Application/Queries/GetNotifications/
// ==================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinVault.NotificationService.Domain.Interfaces;
using MediatR;

namespace FinVault.NotificationService.Application.Queries.GetNotifications;

public class GetNotificationsQueryHandler 
    : IRequestHandler<GetNotificationsQuery, IEnumerable<NotificationDto>>
{
    private readonly INotificationRepository _repository;

    public GetNotificationsQueryHandler(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<NotificationDto>> Handle(
        GetNotificationsQuery request, 
        CancellationToken cancellationToken)
    {
        var alerts = await _repository.GetByUserIdAsync(request.UserId);

        return alerts.Select(a => new NotificationDto(
            a.Id,
            a.Message,
            a.Type,
            a.SentDate,
            a.IsRead,
            a.ActionUrl
        ));
    }
}
