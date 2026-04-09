using System;
using MediatR;

namespace FinVault.CardService.Application.Commands.ApproveCardAdmin;

public record ApproveCardAdminCommand(Guid CardId) : IRequest<bool>;
