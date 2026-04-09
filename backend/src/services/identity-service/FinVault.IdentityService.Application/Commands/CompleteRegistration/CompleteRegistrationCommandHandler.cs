using FinVault.IdentityService.Domain.Entities;
using FinVault.IdentityService.Domain.Interfaces;
using FinVault.Shared.Contracts.Messages;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinVault.IdentityService.Application.Commands.CompleteRegistration;

/// <summary>
/// Step 2 of registration:
/// - Loads PendingRegistration for this email
/// - Verifies the submitted OTP against stored hash
/// - Creates the actual User account with verified email
/// - Cleans up PendingRegistration record
/// - Publishes welcome message
/// </summary>
public class CompleteRegistrationCommandHandler
    : IRequestHandler<CompleteRegistrationCommand, CompleteRegistrationResult>
{
    private readonly IUserRepository _users;
    private readonly IPendingRegistrationRepository _pending;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CompleteRegistrationCommandHandler> _logger;

    public CompleteRegistrationCommandHandler(
        IUserRepository users,
        IPendingRegistrationRepository pending,
        IPublishEndpoint publishEndpoint,
        ILogger<CompleteRegistrationCommandHandler> logger)
    {
        _users           = users;
        _pending         = pending;
        _publishEndpoint = publishEndpoint;
        _logger          = logger;
    }

    public async Task<CompleteRegistrationResult> Handle(
        CompleteRegistrationCommand cmd,
        CancellationToken ct)
    {
        var email = cmd.Email.ToLowerInvariant();
        _logger.LogInformation("Completing registration for {Email}", email);

        // ── 1. LOAD PENDING REGISTRATION ────────────────────────────────
        var pending = await _pending.GetByEmailAsync(email, ct);
        if (pending is null)
        {
            _logger.LogWarning("No pending registration found for {Email}", email);
            return new CompleteRegistrationResult(false,
                "No pending registration found. Please start registration first.");
        }

        // ── 2. CHECK EXPIRY ─────────────────────────────────────────────
        if (pending.ExpiresAt < DateTimeOffset.UtcNow)
        {
            _logger.LogWarning("Registration OTP expired for {Email}", email);
            await _pending.DeleteAsync(email, ct);
            return new CompleteRegistrationResult(false,
                "OTP has expired. Please start registration again.");
        }

        // ── 3. VERIFY OTP ───────────────────────────────────────────────
        if (!BCrypt.Net.BCrypt.Verify(cmd.Code, pending.OtpCodeHash))
        {
            _logger.LogWarning("Invalid OTP for registration {Email}", email);
            return new CompleteRegistrationResult(false, "Invalid OTP code.");
        }

        // ── 4. CHECK IF USER ALREADY EXISTS ──────────────────────────────
        var existing = await _users.GetByEmailAsync(email, ct);
        if (existing is not null)
        {
            _logger.LogWarning("User already exists during registration completion: {Email}", email);
            await _pending.DeleteAsync(email, ct);
            return new CompleteRegistrationResult(false,
                "An account with this email already exists.");
        }

        // ── 5. CREATE VERIFIED USER ──────────────────────────────────────
        var user = new User
        {
            Id              = Guid.NewGuid(),
            Email           = email,
            FirstName       = pending.FirstName,
            LastName        = pending.LastName,
            PasswordHash    = pending.PasswordHash,  // Already hashed in step 1
            IsEmailVerified = true,                   // Email confirmed via OTP
            IsActive        = true,
            CreatedAt       = DateTimeOffset.UtcNow,
            UpdatedAt       = DateTimeOffset.UtcNow
        };

        await _users.AddAsync(user, ct);
        _logger.LogInformation("User created: {UserId} ({Email})", user.Id, user.Email);

        // ── 6. CLEAN UP PENDING REGISTRATION ─────────────────────────────
        await _pending.DeleteAsync(email, ct);

        // ── 7. PUBLISH WELCOME EVENT ─────────────────────────────────────
        await _publishEndpoint.Publish(
            new OtpRequestedMessage(
                user.Email,
                "WELCOME",
                "WelcomeNewUser",
                cmd.CorrelationId),
            ct);

        _logger.LogInformation("Registration completed successfully for {Email}", user.Email);

        return new CompleteRegistrationResult(
            true,
            "Registration successful! Welcome to FinVault.",
            user.Id,
            user.Email);
    }
}
