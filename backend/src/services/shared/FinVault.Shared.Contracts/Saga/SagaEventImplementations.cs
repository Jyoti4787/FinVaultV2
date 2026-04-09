namespace FinVault.Shared.Contracts.Saga;

public record TransactionInitiatedEvent(Guid TransactionId, Guid UserId, Guid CardId, decimal Amount, string Currency) : TransactionInitiated
{
    Guid TransactionInitiated.TransactionId => TransactionId;
    Guid TransactionInitiated.UserId => UserId;
    Guid TransactionInitiated.CardId => CardId;
    decimal TransactionInitiated.Amount => Amount;
    string TransactionInitiated.Currency => Currency;
}

public record CardValidatedEvent(Guid TransactionId, bool IsValid) : CardValidated
{
    Guid CardValidated.TransactionId => TransactionId;
    bool CardValidated.IsValid => IsValid;
}

public record PaymentProcessedEvent(Guid TransactionId, bool Success, string? Reference) : PaymentProcessed
{
    Guid PaymentProcessed.TransactionId => TransactionId;
    bool PaymentProcessed.Success => Success;
    string? PaymentProcessed.Reference => Reference;
}

public record TransactionRecordedEvent(Guid TransactionId, bool Success) : TransactionRecorded
{
    Guid TransactionRecorded.TransactionId => TransactionId;
    bool TransactionRecorded.Success => Success;
}

public record NotificationSentEvent(Guid TransactionId, bool Success) : NotificationSent
{
    Guid NotificationSent.TransactionId => TransactionId;
    bool NotificationSent.Success => Success;
}

public record TransactionFailedEvent(Guid TransactionId, string Reason) : TransactionFailed
{
    Guid TransactionFailed.TransactionId => TransactionId;
    string TransactionFailed.Reason => Reason;
}

public record CardValidationRequestedEvent(Guid TransactionId, Guid CardId, decimal Amount) : CardValidationRequested
{
    Guid CardValidationRequested.TransactionId => TransactionId;
    Guid CardValidationRequested.CardId => CardId;
    decimal CardValidationRequested.Amount => Amount;
}

public record PaymentRequestedEvent(Guid TransactionId, Guid UserId, Guid CardId, decimal Amount, string Currency) : PaymentRequested
{
    Guid PaymentRequested.TransactionId => TransactionId;
    Guid PaymentRequested.UserId => UserId;
    Guid PaymentRequested.CardId => CardId;
    decimal PaymentRequested.Amount => Amount;
    string PaymentRequested.Currency => Currency;
}

public record TransactionRecordRequestedEvent(Guid TransactionId, Guid UserId, Guid CardId, decimal Amount, string Currency) : TransactionRecordRequested
{
    Guid TransactionRecordRequested.TransactionId => TransactionId;
    Guid TransactionRecordRequested.UserId => UserId;
    Guid TransactionRecordRequested.CardId => CardId;
    decimal TransactionRecordRequested.Amount => Amount;
    string TransactionRecordRequested.Currency => Currency;
}

public record NotificationRequestedEvent(Guid TransactionId, Guid UserId, decimal Amount, string Type) : NotificationRequested
{
    Guid NotificationRequested.TransactionId => TransactionId;
    Guid NotificationRequested.UserId => UserId;
    decimal NotificationRequested.Amount => Amount;
    string NotificationRequested.Type => Type;
}

public record PaymentReversalRequestedEvent(Guid TransactionId, decimal Amount) : PaymentReversalRequested
{
    Guid PaymentReversalRequested.TransactionId => TransactionId;
    decimal PaymentReversalRequested.Amount => Amount;
}

public record NotificationReversalRequestedEvent(Guid TransactionId) : NotificationReversalRequested
{
    Guid NotificationReversalRequested.TransactionId => TransactionId;
}