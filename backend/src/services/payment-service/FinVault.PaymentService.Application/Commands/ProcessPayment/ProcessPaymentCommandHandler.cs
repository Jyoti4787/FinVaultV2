// ==================================================================
// FILE : ProcessPaymentCommandHandler.cs
// LAYER: Application (Commands)
// ==================================================================

using FinVault.PaymentService.Domain.Entities;
using FinVault.PaymentService.Domain.Interfaces;
using FinVault.Shared.Contracts.Messages;
using MassTransit;
using MediatR;

namespace FinVault.PaymentService.Application.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler 
    : IRequestHandler<ProcessPaymentCommand, ProcessPaymentResult>
{
    private readonly IPaymentRepository  _repository;
    private readonly IPublishEndpoint    _publishEndpoint;
    private readonly IPaymentOtpVerifier _otpVerifier;

    public ProcessPaymentCommandHandler(
        IPaymentRepository  repository,
        IPublishEndpoint    publishEndpoint,
        IPaymentOtpVerifier otpVerifier)
    {
        _repository      = repository;
        _publishEndpoint = publishEndpoint;
        _otpVerifier     = otpVerifier;
    }

    public async Task<ProcessPaymentResult> Handle(
        ProcessPaymentCommand request, 
        CancellationToken cancellationToken)
    {
        // ── STEP 0: VERIFY OTP BEFORE DOING ANYTHING ─────────────
        // Implementation lives in Infrastructure (HttpClient call to identity-service)
        var otpValid = await _otpVerifier.VerifyAsync(
            request.Email, request.OtpCode, "Payment", cancellationToken);

        if (!otpValid)
            throw new UnauthorizedAccessException("Invalid or expired Payment OTP.");

        // 1. Create payment record as "Pending"
        var payment = new Payment
        {
            Id              = Guid.NewGuid(),
            UserId          = request.UserId,
            CardId          = request.CardId,
            Amount          = request.Amount,
            Currency        = request.Currency,
            Status          = PaymentStatus.Pending,
            TransactionDate = DateTime.UtcNow
        };

        await _repository.AddAsync(payment);

        // 2. Simulate external bank API call
        var (isSuccess, externalId, authCode) = await SimulateBankProcess(request.Amount);

        // 3. Update our record with the result
        if (isSuccess)
        {
            payment.Status                = PaymentStatus.Success;
            payment.ExternalTransactionId = externalId;
            payment.AuthorizationCode     = authCode;
            
            // 4. START THE SAGA
            await _publishEndpoint.Publish(new ProcessPaymentSagaRequested(
                payment.Id,
                payment.CardId,
                payment.UserId,
                request.StatementId,
                payment.Amount,
                $"Payment for statement {request.StatementId}"), cancellationToken);
        }
        else
        {
            payment.Status        = PaymentStatus.Failed;
            payment.FailureReason = "Bank declined transaction";
        }

        await _repository.UpdateAsync(payment);

        return new ProcessPaymentResult(
            payment.Id,
            payment.Status.ToString(),
            payment.ExternalTransactionId,
            isSuccess ? "Payment started (Saga active)" : "Payment failed: Bank declined."
        );
    }

    private async Task<(bool, string, string)> SimulateBankProcess(decimal amount)
    {
        await Task.Delay(500);
        bool success = (amount != 9999); 
        return (success, "TXN_" + Guid.NewGuid().ToString("N")[..10].ToUpper(), "AUTH_" + new Random().Next(100000, 999999));
    }
}
