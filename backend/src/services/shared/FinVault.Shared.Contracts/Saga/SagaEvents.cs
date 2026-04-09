namespace FinVault.Shared.Contracts.Saga;

public interface CardValidated
{
    Guid TransactionId { get; }
    bool IsValid { get; }
}

public interface PaymentProcessed
{
    Guid TransactionId { get; }
    bool Success { get; }
    string? Reference { get; }
}

public interface TransactionRecorded
{
    Guid TransactionId { get; }
    bool Success { get; }
}

public interface NotificationSent
{
    Guid TransactionId { get; }
    bool Success { get; }
}

public interface TransactionFailed
{
    Guid TransactionId { get; }
    string Reason { get; }
}

public interface CardValidationRequested
{
    Guid TransactionId { get; }
    Guid CardId { get; }
    decimal Amount { get; }
}

public interface PaymentRequested
{
    Guid TransactionId { get; }
    Guid UserId { get; }
    Guid CardId { get; }
    decimal Amount { get; }
    string Currency { get; }
}

public interface TransactionRecordRequested
{
    Guid TransactionId { get; }
    Guid UserId { get; }
    Guid CardId { get; }
    decimal Amount { get; }
    string Currency { get; }
}

public interface NotificationRequested
{
    Guid TransactionId { get; }
    Guid UserId { get; }
    decimal Amount { get; }
    string Type { get; }
}

public interface PaymentReversalRequested
{
    Guid TransactionId { get; }
    decimal Amount { get; }
}

public interface NotificationReversalRequested
{
    Guid TransactionId { get; }
}