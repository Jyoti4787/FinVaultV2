using FinVault.IdentityService.Domain.Entities;
using FinVault.IdentityService.Domain.Interfaces;
using FinVault.Shared.Contracts.Messages;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinVault.IdentityService.Application.Commands.RegisterUser;

public class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository    _users;
    private readonly IPendingRegistrationRepository _pending;
    private readonly IPublishEndpoint   _publishEndpoint;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IUserRepository users,
        IPendingRegistrationRepository pending,
        IPublishEndpoint publishEndpoint,
        ILogger<RegisterUserCommandHandler> logger)
    {
        _users           = users;
        _pending         = pending;
        _publishEndpoint = publishEndpoint;
        _logger          = logger;
    }

    public async Task<RegisterUserResult> Handle(
        RegisterUserCommand cmd,
        CancellationToken ct)
    {
        var email = cmd.Email.ToLowerInvariant();
        _logger.LogInformation("Registration attempt for {Email}", email);

        // ── 1. REQUIRE VERIFIED PENDING REGISTRATION ─────────────────
        // User MUST have completed the OTP verification step first.
        var pendingRecord = await _pending.GetByEmailAsync(email, ct);

        if (pendingRecord is null)
            throw new InvalidOperationException(
                "Please request and verify the email OTP before registering. " +
                "Call POST /register/send-otp first.");

        if (!pendingRecord.EmailVerified)
            throw new InvalidOperationException(
                "Email address has not been verified. " +
                "Please verify the OTP sent to your email first.");

        // ── 2. DUPLICATE CHECK ────────────────────────────────────────
        var existing = await _users.GetByEmailAsync(email, ct);
        if (existing is not null)
        {
            _logger.LogWarning("Registration rejected — email already exists: {Email}", email);
            throw new InvalidOperationException($"An account with email '{email}' already exists.");
        }

        // ── 3. CREATE VERIFIED USER ───────────────────────────────────
        var user = new User
        {
            Id              = Guid.NewGuid(),
            Email           = email,
            FirstName       = cmd.FirstName,
            LastName        = cmd.LastName,
            PasswordHash    = BCrypt.Net.BCrypt.HashPassword(cmd.Password),
            IsEmailVerified = true,    // already confirmed via OTP step
            IsActive        = true,
            CreatedAt       = DateTimeOffset.UtcNow,
            UpdatedAt       = DateTimeOffset.UtcNow
        };

        await _users.AddAsync(user, ct);
        _logger.LogInformation("User created: {UserId} ({Email})", user.Id, user.Email);

        // ── 4. CLEAN UP PENDING REGISTRATION RECORD ───────────────────
        await _pending.DeleteAsync(email, ct);

        // ── 5. PUBLISH WELCOME EVENT ──────────────────────────────────
        await _publishEndpoint.Publish(
            new OtpRequestedMessage(
                user.Email,
                "WELCOME",          // no raw OTP needed; notification-service sends welcome email
                "WelcomeNewUser",
                cmd.CorrelationId),
            ct);

        _logger.LogInformation("User registration complete for {Email}", user.Email);

        return new RegisterUserResult(
            user.Id,
            user.Email,
            "Registration successful! Welcome to FinVault.");
    }
}


