using FinVault.IdentityService.Domain.Entities;
using FinVault.IdentityService.Domain.Interfaces;
using FinVault.Shared.Contracts.Messages;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinVault.IdentityService.Application.Commands.InitiateRegistration;

/// <summary>
/// Step 1 of registration:
/// - Validates email is not already registered
/// - Stores user data + OTP hash in PendingRegistration
/// - Publishes OtpRequestedMessage → notification-service emails the code
/// </summary>
public class InitiateRegistrationCommandHandler
    : IRequestHandler<InitiateRegistrationCommand, InitiateRegistrationResult>
{
    private readonly IUserRepository _users;
    private readonly IPendingRegistrationRepository _pending;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<InitiateRegistrationCommandHandler> _logger;

    public InitiateRegistrationCommandHandler(
        IUserRepository users,
        IPendingRegistrationRepository pending,
        IPublishEndpoint publishEndpoint,
        ILogger<InitiateRegistrationCommandHandler> logger)
    {
        _users           = users;
        _pending         = pending;
        _publishEndpoint = publishEndpoint;
        _logger          = logger;
    }

    public async Task<InitiateRegistrationResult> Handle(
        InitiateRegistrationCommand cmd,
        CancellationToken ct)
    {
        var email = cmd.Email.ToLowerInvariant();
        _logger.LogInformation("Registration initiated for {Email}", email);

        // ── 1. REJECT ALREADY-REGISTERED EMAILS ─────────────────────────
        var existing = await _users.GetByEmailAsync(email, ct);
        if (existing is not null)
            throw new InvalidOperationException($"An account with email '{email}' already exists.");

        // ── TEMPORARY: SKIP OTP - DIRECTLY CREATE USER ──────────────────
        _logger.LogWarning(">>> DEVELOPMENT MODE: OTP DISABLED - Creating user directly <<<");
        
        var user = new User
        {
            Id              = Guid.NewGuid(),
            Email           = email,
            FirstName       = cmd.FirstName,
            LastName        = cmd.LastName,
            PasswordHash    = BCrypt.Net.BCrypt.HashPassword(cmd.Password),
            IsEmailVerified = true,  // Skip email verification for now
            IsActive        = true,
            CreatedAt       = DateTimeOffset.UtcNow,
            UpdatedAt       = DateTimeOffset.UtcNow
        };

        await _users.AddAsync(user, ct);
        _logger.LogInformation("User created directly (OTP skipped): {UserId} ({Email})", user.Id, user.Email);

        return new InitiateRegistrationResult(
            $"Registration successful! User created with ID: {user.Id}");

        /* ORIGINAL OTP CODE - COMMENTED OUT
        // ── 2. GENERATE 6-DIGIT OTP ─────────────────────────────────────
        var raw  = Random.Shared.Next(100_000, 999_999).ToString();
        var otpHash = BCrypt.Net.BCrypt.HashPassword(raw);

        // ── 3. HASH PASSWORD ────────────────────────────────────────────
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(cmd.Password);

        // ── 4. STORE IN PendingRegistration ──────────────────────────────
        // Delete any stale pending rows, then insert fresh one with all user data
        await _pending.DeleteAsync(email, ct);
        await _pending.AddAsync(new PendingRegistration
        {
            Id            = Guid.NewGuid(),
            Email         = email,
            PasswordHash  = passwordHash,
            FirstName     = cmd.FirstName,
            LastName      = cmd.LastName,
            OtpCodeHash   = otpHash,
            EmailVerified = false,
            ExpiresAt     = DateTimeOffset.UtcNow.AddMinutes(15),
            CreatedAt     = DateTimeOffset.UtcNow
        }, ct);

        // ── 5. PUBLISH → notification-service emails the OTP ─────────────
        await _publishEndpoint.Publish(
            new OtpRequestedMessage(email, raw, "SignupVerification", cmd.CorrelationId),
            ct);

        // DEV helper — see code in logs without needing email
        _logger.LogWarning(
            ">>> DEV REGISTRATION OTP <<< Email: {Email} | Code: {Code} | Expires in 15 min",
            email, raw);

        return new InitiateRegistrationResult(
            "Registration initiated. A verification code has been sent to your email. Valid for 15 minutes.");
        */
    }
}
