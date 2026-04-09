// ==================================================================
// FILE : VerifyLoginOtpCommand.cs
// LAYER: Application (Commands)
// WHAT IS THIS?
// Step 2 of the login flow.
// User types the OTP from their email → we verify it → issue JWT.
// ==================================================================

using MediatR;

namespace FinVault.IdentityService.Application.Commands.VerifyLoginOtp;

// Published by : AuthController POST /api/identity/auth/login/verify-otp
// Consumed by  : VerifyLoginOtpCommandHandler
public record VerifyLoginOtpCommand(
    string Email,
    string OtpCode,
    Guid   CorrelationId
) : IRequest<VerifyLoginOtpResult>;

// Full JWT result — same as the old LoginUserResult
public record VerifyLoginOtpResult(
    string AccessToken,
    string RefreshToken,
    Guid   UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTimeOffset ExpiresAt
);
