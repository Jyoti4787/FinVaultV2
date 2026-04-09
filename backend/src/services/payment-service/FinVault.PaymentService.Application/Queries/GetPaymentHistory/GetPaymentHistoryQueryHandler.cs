// ==================================================================
// FILE : GetPaymentHistoryQueryHandler.cs
// LAYER: Application (Queries)
// PATH : payment-service/FinVault.PaymentService.Application/Queries/GetPaymentHistory/
//
// WHAT IS THIS?
// This is the worker that handles the "GetPaymentHistoryQuery".
// It talks to our IPaymentRepository to get all the digital receipts!
//
// Published by : MediatR (automatically finds this class)
// Consumed by  : PaymentsController
// ==================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinVault.PaymentService.Domain.Interfaces;
using MediatR;

namespace FinVault.PaymentService.Application.Queries.GetPaymentHistory;

public class GetPaymentHistoryQueryHandler 
    : IRequestHandler<GetPaymentHistoryQuery, IEnumerable<PaymentHistoryDto>>
{
    private readonly IPaymentRepository _repository;

    public GetPaymentHistoryQueryHandler(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PaymentHistoryDto>> Handle(
        GetPaymentHistoryQuery request, 
        CancellationToken cancellationToken)
    {
        // 1. Fetch our data from the repository (database)
        var payments = await _repository.GetByUserIdAsync(request.UserId);

        // 2. Convert from the internal Payment entity (Domain)
        //    to a simplified DTO for the Angular app
        return payments.Select(p => new PaymentHistoryDto(
            p.Id,
            p.CardId,
            p.Amount,
            p.Status,
            p.TransactionDate,
            p.ExternalTransactionId
        ));
    }
}
