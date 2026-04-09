using FinVault.IdentityService.Domain.Entities;
using FinVault.IdentityService.Domain.Interfaces;
using FinVault.Shared.Contracts.Messages;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinVault.IdentityService.Application.Commands.SendOTP;

/// <summary>
/// Handles OTP generation for all purposes:
/// Login, PasswordReset, Payment, SignupVerification.
///
/// Features:
///   ✅ Rate limiting  — max 3 OTPs per 5 minutes per user+purpose
///   ✅ Resend support — invalidates all old active OTPs before issuing new one
///   ✅ Structured logging — tracks every step for observability
///   ✅ RabbitMQ publish — notification-service sends the actual email
/// </summary>
public class SendOTPCommandHandler
    : IRequestHandler<SendOTPCommand, SendOTPResult>
{
    // Rate limit: max 3 OTPs per 5-minute window per user+purpose
    private const int    MaxOtpsPerWindow  = 3;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(5);

    private readonly IUserRepository    _users;
    private readonly IOTPCodeRepository _otps;
    private readonly IPublishEndpoint   _publishEndpoint;
    private readonly ILogger<SendOTPCommandHandler> _logger;

    public SendOTPCommandHandler(
        IUserRepository users,
        IOTPCodeRepository otps,
        IPublishEndpoint publishEndpoint,
        ILogger<SendOTPCommandHandler> logger)
    {
        _users           = users;
        _otps            = otps;
        _publishEndpoint = publishEndpoint;
        _logger          = logger;
    }

    public async Task<SendOTPResult> Handle(
        SendOTPCommand cmd,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "OTP requested — Email: {Email}, Purpose: {Purpose}, CorrelationId: {CorrelationId}",
            cmd.Email, cmd.Purpose, cmd.CorrelationId);

        // ── 1. FIND USER ──────────────────────────────────────────
        var user = await _users.GetByEmailAsync(cmd.Email.ToLowerInvariant(), ct)
            ?? throw new KeyNotFoundException($"No account found for email '{cmd.Email}'.");

        // ── 2. RATE LIMIT CHECK ───────────────────────────────────
        // Prevent abuse: max 3 OTPs per 5 minutes per user+purpose
        var recentCount = await _otps.CountRecentAsync(
            user.Id, cmd.Purpose, RateLimitWindow, ct);

        if (recentCount >= MaxOtpsPerWindow)
        {
            _logger.LogWarning(
                "OTP rate limit hit — UserId: {UserId}, Purpose: {Purpose}, Count: {Count}",
                user.Id, cmd.Purpose, recentCount);

            throw new InvalidOperationException(
                $"Too many OTP requests. Please wait {(int)RateLimitWindow.TotalMinutes} minutes before trying again.");
        }

        // ── 3. RESEND — INVALIDATE ALL OLD ACTIVE OTPs ───────────
        // If user clicks "Resend", old codes become invalid immediately.
        // This prevents someone from using a previously sent code after a resend.
        await _otps.InvalidateAllActiveAsync(user.Id, cmd.Purpose, ct);

        _logger.LogDebug(
            "Old active OTPs invalidated — UserId: {UserId}, Purpose: {Purpose}",
            user.Id, cmd.Purpose);

        // ── 4. GENERATE NEW 6-DIGIT OTP ──────────────────────────
        // Random.Shared is thread-safe in .NET 6+
        var raw  = Random.Shared.Next(100_000, 999_999).ToString();
        var hash = BCrypt.Net.BCrypt.HashPassword(raw);

        // ── 5. SAVE HASHED OTP TO DATABASE ───────────────────────
        var otpId = Guid.NewGuid();
        await _otps.AddAsync(new OTPCode
        {
            Id        = otpId,
            UserId    = user.Id,
            CodeHash  = hash,
            Purpose   = cmd.Purpose,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            CreatedAt = DateTimeOffset.UtcNow
        }, ct);

        _logger.LogInformation(
            "OTP saved — OtpId: {OtpId}, UserId: {UserId}, Purpose: {Purpose}, ExpiresAt: {ExpiresAt}",
            otpId, user.Id, cmd.Purpose, DateTimeOffset.UtcNow.AddMinutes(5));

        // ── 6. PUBLISH TO RABBITMQ ────────────────────────────────
        // notification-service OtpRequestedConsumer picks this up
        // and sends the HTML email with the raw OTP code.
        // MassTransit retry (3x, 5s interval) is configured on the consumer endpoint.
        await _publishEndpoint.Publish(
            new OtpRequestedMessage(
                user.Email,
                raw,           // raw code goes in the email; only hash is in DB
                cmd.Purpose,
                cmd.CorrelationId),
            ct);

        _logger.LogInformation(
            "OtpRequestedMessage published — Email: {Email}, Purpose: {Purpose}",
            user.Email, cmd.Purpose);

        // ── DEV HELPER: log raw OTP to console so you can test without email ──
        _logger.LogWarning(
            ">>> DEV OTP <<< Email: {Email} | Purpose: {Purpose} | Code: {Code} | Expires in 5 min",
            user.Email, cmd.Purpose, raw);

        return new SendOTPResult("OTP sent to your email. Valid for 5 minutes.");
    }
}
