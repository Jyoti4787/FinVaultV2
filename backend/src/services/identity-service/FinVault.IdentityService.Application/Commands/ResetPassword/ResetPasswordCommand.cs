using MediatR;

namespace FinVault.IdentityService.Application.Commands.ResetPassword;

// WHAT IS THE PASSWORD RESET FLOW?
// 1. User clicks "Forgot Password"
// 2. Calls SendOTP → gets email with code
// 3. User types new password + the code
// 4. Calls this command
// 5. We verify the code → save new password
//    → logout from all devices

// Published by : AuthController POST /api/identity/auth/reset-password
// Consumed by  : ResetPasswordCommandHandler
public record ResetPasswordCommand(
    string Email,

    // The 6-digit code from email
    string OtpCode,

    // The new password they want to set
    string NewPassword,

    Guid CorrelationId
) : IRequest<ResetPasswordResult>;

public record ResetPasswordResult(string Message);