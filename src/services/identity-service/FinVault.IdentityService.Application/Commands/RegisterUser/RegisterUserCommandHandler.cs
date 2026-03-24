using FinVault.IdentityService.Domain.Entities;
using FinVault.IdentityService.Domain.Events;
using FinVault.IdentityService.Domain.Interfaces;
using MediatR;

namespace FinVault.IdentityService.Application.Commands.RegisterUser;

// Published by : MediatR routes here automatically
//                when RegisterUserCommand is sent
// Consumed by  : Nobody — this is the final stop
//                saves user to DB and fires domain event

public class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    // IUserRepository = the contract from Domain layer
    // We don't know or care about EF Core here
    // We just use the contract
    private readonly IUserRepository _users;

    // IPublisher = MediatR's way to fire domain events
    // After saving user we shout "UserRegistered!"
    private readonly IPublisher _publisher;

    public RegisterUserCommandHandler(
        IUserRepository users,
        IPublisher publisher)
    {
        _users     = users;
        _publisher = publisher;
    }

    public async Task<RegisterUserResult> Handle(
        RegisterUserCommand cmd,
        CancellationToken ct)
    {
        // Step 1 — Check if email already exists
        // If yes throw exception
        // ExceptionMiddleware catches it and returns 409 Conflict
        if (await _users.ExistsByEmailAsync(cmd.Email, ct))
            throw new InvalidOperationException(
                "Email already registered.");

        // Step 2 — Hash the password
        // BCrypt.HashPassword scrambles "MyPassword123"
        // into something like "$2a$12$abc123xyz..."
        // workFactor 12 means it takes ~0.3 seconds to hash
        // This is SLOW on purpose — makes brute force attacks hard
        var hash = BCrypt.Net.BCrypt.HashPassword(
            cmd.Password, workFactor: 12);

        // Step 3 — Create the User object
        var user = new User
        {
            Id           = Guid.NewGuid(),
            // Always store email in lowercase
            // So "Jyoti@gmail.com" and "jyoti@gmail.com" are same
            Email        = cmd.Email.ToLowerInvariant(),
            PasswordHash = hash,
            FirstName    = cmd.FirstName,
            LastName     = cmd.LastName,
            Role         = "User",
            CreatedAt    = DateTimeOffset.UtcNow
        };

        // Step 4 — Save to database
        await _users.AddAsync(user, ct);

        // Step 5 — Fire domain event
        // This goes to UserRegisteredPublisher
        // Which sends it to RabbitMQ
        // Which notification-service picks up
        // Which sends welcome email to user
        await _publisher.Publish(
            new UserRegisteredDomainEvent(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                cmd.CorrelationId), ct);

        return new RegisterUserResult(
            user.Id,
            user.Email,
            "Registration successful.");
    }
}