using FinVault.NotificationService.Domain.Entities;
using FinVault.NotificationService.Domain.Interfaces;
using MediatR;

namespace FinVault.NotificationService.Application.Commands.CreateTicket;

public record CreateTicketCommand(
    Guid UserId,
    string IssueType,
    string Message) : IRequest<CreateTicketResult>;

public record CreateTicketResult(
    Guid TicketId,
    string Status,
    DateTime CreatedAt);

public class CreateTicketCommandHandler 
    : IRequestHandler<CreateTicketCommand, CreateTicketResult>
{
    private readonly ISupportTicketRepository _repository;

    public CreateTicketCommandHandler(ISupportTicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateTicketResult> Handle(
        CreateTicketCommand request, 
        CancellationToken ct)
    {
        var ticket = SupportTicket.Create(
            request.UserId, 
            request.IssueType, 
            request.Message);

        await _repository.AddAsync(ticket);

        return new CreateTicketResult(
            ticket.Id,
            ticket.Status,
            ticket.CreatedAt);
    }
}
