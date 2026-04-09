using MediatR;

namespace FinVault.IdentityService.Application.Commands.RegisterUser;

// WHAT IS A COMMAND?
// A command is like a TASK SLIP you hand to someone
// "Here is what I want done, here is the data you need"
// MediatR reads the task slip and finds the right handler
//
// IRequest<RegisterUserResult> means:
// "When this command is handled, give me back a RegisterUserResult"

// Published by : AuthController POST /api/identity/auth/register
// Consumed by  : RegisterUserCommandHandler
public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    Guid CorrelationId
) : IRequest<RegisterUserResult>;

// This is what the handler sends back after registration
public record RegisterUserResult(
    Guid UserId,
    string Email,
    string Message);