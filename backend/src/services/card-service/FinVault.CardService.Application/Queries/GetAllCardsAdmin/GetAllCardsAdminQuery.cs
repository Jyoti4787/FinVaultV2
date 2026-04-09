using FinVault.CardService.Application.Queries.GetCards;
using MediatR;

namespace FinVault.CardService.Application.Queries.GetAllCardsAdmin;

public record GetAllCardsAdminQuery() : IRequest<List<CardSummary>>;
