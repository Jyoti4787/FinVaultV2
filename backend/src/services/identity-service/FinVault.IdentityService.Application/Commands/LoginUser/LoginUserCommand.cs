using MediatR;

namespace FinVault.IdentityService.Application.Commands.LoginUser;

// WHAT HAPPENS WHEN YOU LOGIN?
// STEP 1: POST /auth/login  → validates credentials → sends OTP email
// STEP 2: POST /auth/login/verify-otp  → validates OTP → returns JWT
// This makes login much more secure — password alone is not enough!

// Published by : AuthController POST /api/identity/auth/login
// Consumed by  : LoginUserCommandHandler
public record LoginUserCommand(
    string Email,
    string Password,
    Guid   CorrelationId
) : IRequest<LoginUserResult>;

// Step 1 login just says "OTP sent" — no token yet
// TEMPORARY: Added optional fields for direct JWT login (OTP disabled)
public record LoginUserResult(
    // true when OTP has been sent to user's email
    // frontend should then show the OTP input screen
    bool OtpRequired,

    // Message to show the user
    string Message,
    
    // TEMPORARY: For direct login without OTP
    Guid? UserId = null,
    string? Email = null,
    string? Role = null
);