// ==================================================================
// FILE : IdentityOtpVerifier.cs
// LAYER: Infrastructure / OtpVerification
// WHAT IS THIS?
// Implements IPaymentOtpVerifier by calling identity-service HTTP API.
// POST /api/identity/auth/mfa/verify with { Email, Code, Purpose }
// ==================================================================

using System.Net.Http.Json;
using System.Text.Json;
using FinVault.PaymentService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FinVault.PaymentService.Infrastructure.OtpVerification;

public class IdentityOtpVerifier : IPaymentOtpVerifier
{
    private readonly HttpClient    _httpClient;
    private readonly IConfiguration _config;

    public IdentityOtpVerifier(
        IHttpClientFactory httpClientFactory,
        IConfiguration     config)
    {
        _httpClient = httpClientFactory.CreateClient("IdentityService");
        _config     = config;
    }

    public async Task<bool> VerifyAsync(
        string email,
        string otpCode,
        string purpose,
        CancellationToken ct = default)
    {
        // Saga internal path: OTP was already verified at controller level
        if (string.IsNullOrEmpty(otpCode) || string.IsNullOrEmpty(email))
            return true;

        var baseUrl = _config["IdentityService:BaseUrl"] ?? "http://localhost:5001";

        var response = await _httpClient.PostAsJsonAsync(
            $"{baseUrl}/api/identity/auth/mfa/verify",
            new { Email = email, Code = otpCode, Purpose = purpose },
            ct);

        if (!response.IsSuccessStatusCode)
            return false;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);

        // Response: { success: true, data: { success: true, message: "..." } }
        return json.TryGetProperty("data", out var data) &&
               data.TryGetProperty("success", out var success) &&
               success.GetBoolean();
    }
}
