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

public record LoginUserResult(
    // true when OTP has been sent to user's email
    // frontend should then show the OTP input screen
    bool OtpRequired,

    // Message to show the user
    string Message,
    
    // Optional properties used for direct login (e.g., Admin)
    string? AccessToken = null,
    string? RefreshToken = null,
    Guid? UserId = null,
    string? Email = null,
    string? Role = null
);