// ==================================================================
// FILE : OtpRequestedMessage.cs
// LAYER: Shared Contracts (Messages)
// WHAT IS THIS?
// The RabbitMQ message that identity-service publishes
// when an OTP is generated. notification-service listens
// for this and sends the actual email to the user.
// ==================================================================

namespace FinVault.Shared.Contracts.Messages;

public record OtpRequestedMessage(
    // User's email address — the "To:" field
    string Email,

    // The raw 6-digit OTP code to put in the email body
    string OtpCode,

    // WHY the OTP was generated:
    // "Login"         → "Your FinVault Login OTP is: 123456"
    // "Payment"       → "Your FinVault Payment OTP is: 123456"
    // "PasswordReset" → "Reset your FinVault password: 123456"
    string Purpose,

    Guid CorrelationId
);
