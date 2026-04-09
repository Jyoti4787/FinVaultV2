using FinVault.IdentityService.Domain.Entities;
using FinVault.IdentityService.Domain.Interfaces;
using FinVault.Shared.Contracts.Messages;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinVault.IdentityService.Application.Commands.SendRegistrationOtp;

/// <summary>
/// Step 1 of registration:
/// - Rejects emails that already have a verified User account
/// - Creates/replaces a PendingRegistration row with a fresh OTP hash
/// - Publishes OtpRequestedMessage → notification-service emails the code
/// </summary>
public class SendRegistrationOtpCommandHandler
    : IRequestHandler<SendRegistrationOtpCommand, SendRegistrationOtpResult>
{
    private readonly IUserRepository _users;
    private readonly IPendingRegistrationRepository _pending;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SendRegistrationOtpCommandHandler> _logger;

    public SendRegistrationOtpCommandHandler(
        IUserRepository users,
        IPendingRegistrationRepository pending,
        IPublishEndpoint publishEndpoint,
        ILogger<SendRegistrationOtpCommandHandler> logger)
    {
        _users           = users;
        _pending         = pending;
        _publishEndpoint = publishEndpoint;
        _logger          = logger;
    }

    public async Task<SendRegistrationOtpResult> Handle(
        SendRegistrationOtpCommand cmd,
        CancellationToken ct)
    {
        var email = cmd.Email.ToLowerInvariant();
        _logger.LogInformation("Registration OTP requested for {Email}", email);

        // ── 1. REJECT ALREADY-REGISTERED EMAILS ─────────────────────────
        var existing = await _users.GetByEmailAsync(email, ct);
        if (existing is not null)
            throw new InvalidOperationException($"An account with email '{email}' already exists.");

        // ── 2. GENERATE 6-DIGIT OTP ─────────────────────────────────────
        var raw  = Random.Shared.Next(100_000, 999_999).ToString();
        var hash = BCrypt.Net.BCrypt.HashPassword(raw);

        // ── 3. UPSERT PendingRegistration ────────────────────────────────
        // Delete any stale pending rows, then insert fresh one
        await _pending.DeleteAsync(email, ct);
        await _pending.AddAsync(new PendingRegistration
        {
            Id           = Guid.NewGuid(),
            Email        = email,
            OtpCodeHash  = hash,
            EmailVerified = false,
            ExpiresAt    = DateTimeOffset.UtcNow.AddMinutes(15),
            CreatedAt    = DateTimeOffset.UtcNow
        }, ct);

        // ── 4. PUBLISH → notification-service emails the OTP ─────────────
        await _publishEndpoint.Publish(
            new OtpRequestedMessage(email, raw, "SignupVerification", cmd.CorrelationId),
            ct);

        // DEV helper — see code in logs without needing email
        _logger.LogWarning(
            ">>> DEV REGISTRATION OTP <<< Email: {Email} | Code: {Code} | Expires in 15 min",
            email, raw);

        return new SendRegistrationOtpResult(
            "A verification code has been sent to your email. Valid for 15 minutes.");
    }
}
