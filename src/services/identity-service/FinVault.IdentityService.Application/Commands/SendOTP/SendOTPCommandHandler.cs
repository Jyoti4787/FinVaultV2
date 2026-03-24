using FinVault.IdentityService.Domain.Entities;
using FinVault.IdentityService.Domain.Events;
using FinVault.IdentityService.Domain.Interfaces;
using MediatR;

namespace FinVault.IdentityService.Application.Commands.SendOTP;

// WHAT DOES THIS HANDLER DO?
// Step 1 — find the user by email
// Step 2 — generate a random 6-digit number
// Step 3 — hash it (never store raw OTP)
// Step 4 — save hashed OTP to database
// Step 5 — fire event with RAW OTP
//          so notification-service can put it in the email

// Published by : MediatR routes here when SendOTPCommand sent
// Consumed by  : Nobody — saves OTP and fires event for email
public class SendOTPCommandHandler
    : IRequestHandler<SendOTPCommand, SendOTPResult>
{
    private readonly IUserRepository _users;
    private readonly IOTPCodeRepository _otps;

    // IPublisher fires domain events
    // We fire PasswordResetRequestedDomainEvent
    // Publisher picks it up and sends to RabbitMQ
    // notification-service receives it and sends email
    private readonly IPublisher _publisher;

    public SendOTPCommandHandler(
        IUserRepository users,
        IOTPCodeRepository otps,
        IPublisher publisher)
    {
        _users     = users;
        _otps      = otps;
        _publisher = publisher;
    }

    public async Task<SendOTPResult> Handle(
        SendOTPCommand cmd,
        CancellationToken ct)
    {
        // Step 1 — Find user by email
        // If user not found → throw 404
        var user = await _users.GetByEmailAsync(
            cmd.Email.ToLowerInvariant(), ct)
            ?? throw new KeyNotFoundException(
                "User not found.");

        // Step 2 — Generate a random 6-digit number
        // Random.Shared is thread-safe in .NET 6+
        // Next(100000, 999999) gives numbers
        // between 100000 and 999999
        // ToString() converts 123456 to "123456"
        var raw = Random.Shared
            .Next(100000, 999999).ToString();

        // Step 3 — Hash the OTP before saving
        // Same reason as password hashing
        // If database is hacked, hacker cannot read OTPs
        var hash = BCrypt.Net.BCrypt.HashPassword(raw);

        // Step 4 — Save OTP to database
        await _otps.AddAsync(new OTPCode
        {
            Id       = Guid.NewGuid(),
            UserId   = user.Id,
            CodeHash = hash,
            Purpose  = cmd.Purpose,
            // OTP expires in 5 minutes
            // After that it cannot be used even if correct
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            CreatedAt = DateTimeOffset.UtcNow
        }, ct);

        // Step 5 — Fire domain event with RAW OTP
        // WHY raw and not hashed?
        // Because notification-service needs to put
        // "Your OTP is: 123456" in the email
        // We already saved the HASHED version in DB
        // So it's safe to pass raw in the event
        await _publisher.Publish(
            new PasswordResetRequestedDomainEvent(
                user.Id,
                user.Email,
                raw,
                cmd.CorrelationId), ct);

        return new SendOTPResult(
            "OTP sent to your email. Valid for 5 minutes.");
    }
}