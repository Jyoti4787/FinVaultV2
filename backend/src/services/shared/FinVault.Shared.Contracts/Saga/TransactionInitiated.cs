namespace FinVault.Shared.Contracts.Saga;

public interface TransactionInitiated
{
    Guid TransactionId { get; }
    Guid UserId { get; }
    Guid CardId { get; }
    decimal Amount { get; }
    string Currency { get; }
}