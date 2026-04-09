using FinVault.NotificationService.Application.Queries.GetTickets;
using FinVault.NotificationService.Domain.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FinVault.NotificationService.Application.Queries.GetAllTickets;

public record GetAllTicketsQuery() : IRequest<List<TicketDto>>;

public class GetAllTicketsQueryHandler : IRequestHandler<GetAllTicketsQuery, List<TicketDto>>
{
    private readonly ISupportTicketRepository _repository;

    public GetAllTicketsQueryHandler(ISupportTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TicketDto>> Handle(GetAllTicketsQuery request, CancellationToken ct)
    {
        var tickets = await _repository.GetAllAsync();
        
        return tickets.Select(t => new TicketDto(
            t.Id,
            t.UserId,
            t.Subject,
            t.Message,
            t.Status,
            t.AdminComment,
            t.CreatedAt)).ToList();
    }
}
