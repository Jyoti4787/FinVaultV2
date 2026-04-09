using FinVault.NotificationService.Domain.Interfaces;
using MediatR;

namespace FinVault.NotificationService.Application.Queries.GetTickets;

public record GetTicketsQuery(Guid UserId) : IRequest<List<TicketDto>>;

public record TicketDto(
    Guid Id,
    Guid UserId,
    string IssueType,
    string Message,
    string Status,
    string? AdminComment,
    DateTime CreatedAt);

public class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, List<TicketDto>>
{
    private readonly ISupportTicketRepository _repository;

    public GetTicketsQueryHandler(ISupportTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TicketDto>> Handle(GetTicketsQuery request, CancellationToken ct)
    {
        var tickets = await _repository.GetByUserIdAsync(request.UserId);
        
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
