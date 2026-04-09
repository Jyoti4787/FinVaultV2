using FinVault.NotificationService.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FinVault.NotificationService.Application.Commands.ResolveTicket;

public record ResolveTicketCommand(
    Guid TicketId,
    string AdminComment) : IRequest<bool>;

public class ResolveTicketCommandHandler : IRequestHandler<ResolveTicketCommand, bool>
{
    private readonly ISupportTicketRepository _repository;

    public ResolveTicketCommandHandler(ISupportTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(ResolveTicketCommand request, CancellationToken ct)
    {
        var ticket = await _repository.GetByIdAsync(request.TicketId);
        
        if (ticket == null)
            return false;

        ticket.Resolve(request.AdminComment);
        await _repository.UpdateAsync(ticket);

        return true;
    }
}
