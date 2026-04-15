// ==================================================================
// FILE : IdentityOtpVerifier.cs
// LAYER: Infrastructure / OtpVerification
// WHAT IS THIS?
// Implements ICardOtpVerifier by calling identity-service HTTP API.
// POST /api/identity/auth/mfa/verify with { Email, Code, Purpose }
// ==================================================================

using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using FinVault.CardService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FinVault.CardService.Infrastructure.OtpVerification;

public class IdentityOtpVerifier : ICardOtpVerifier
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<IdentityOtpVerifier> _logger;

    public IdentityOtpVerifier(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<IdentityOtpVerifier> logger)
    {
        _httpClient = httpClientFactory.CreateClient("IdentityService");
        _config = config;
        _logger = logger;
    }

    public async Task<bool> VerifyAsync(
        string email,
        string otpCode,
        string purpose,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(otpCode) || string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("OTP verification skipped — empty email or OTP code");
            return false;
        }

        var baseUrl = _config["IdentityService:BaseUrl"] ?? "http://identity-service:8080";

        _logger.LogInformation("Verifying OTP for {Email} with purpose {Purpose}", email, purpose);

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{baseUrl}/api/identity/auth/mfa/verify",
                new { Email = email, Code = otpCode, Purpose = purpose },
                ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OTP verification failed — HTTP {StatusCode}", response.StatusCode);
                return false;
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

            // Response: { success: true, data: { isValid: true, message: "..." } }
            var isValid = json.TryGetProperty("data", out var data) &&
                          data.TryGetProperty("isValid", out var validToken) &&
                          validToken.GetBoolean();

            _logger.LogInformation("OTP verification result for {Email}: {IsValid}", email, isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OTP verification failed for {Email}", email);
            return false;
        }
    }
}
