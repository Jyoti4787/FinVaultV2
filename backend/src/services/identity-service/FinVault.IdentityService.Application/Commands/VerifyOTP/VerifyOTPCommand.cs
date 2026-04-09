using MediatR;

namespace FinVault.IdentityService.Application.Commands.VerifyOTP;

// WHEN IS THIS USED?
// After SendOTP is called → user gets email with "123456"
// User types that code in the app
// App calls this to check if it's correct

// Published by : AuthController POST /api/identity/auth/mfa/verify
// Consumed by  : VerifyOTPCommandHandler
public record VerifyOTPCommand(
    // Which user is verifying
    string Email,

    // The 6-digit code the user typed
    string Code,

    // Which OTP purpose are we checking?
    // Must match the purpose used in SendOTP
    // "Login" OTP cannot be used for "PasswordReset"
    string Purpose,

    Guid CorrelationId
) : IRequest<VerifyOTPResult>;

// What comes back
public record VerifyOTPResult(
    // true = code is correct
    // false = wrong code or expired
    bool IsValid,
    string Message);