using MediatR;

namespace FinVault.IdentityService.Application.Commands.SendOTP;

// WHEN IS THIS USED?
// 1. User forgets password → clicks "Forgot Password"
//    → app calls this → sends OTP to email
// 2. User tries to login from new device
//    → app calls this → sends OTP to email
// 3. User makes payment over Rs.10,000
//    → app calls this → sends OTP to email
//
// The Purpose field tells us WHY the OTP was requested
// So we don't mix up a login OTP with a payment OTP

// Published by : AuthController POST /api/identity/auth/mfa/send
// Consumed by  : SendOTPCommandHandler
public record SendOTPCommand(
    // Which email to send the OTP to
    string Email,

    // WHY is this OTP being sent?
    // "Login" = logging in from new device
    // "Payment" = confirming a big payment
    // "PasswordReset" = resetting password
    string Purpose,

    Guid CorrelationId
) : IRequest<SendOTPResult>;

// Just a simple success message back
public record SendOTPResult(string Message);