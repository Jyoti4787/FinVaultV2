// ==================================================================
// FILE : KeyVaultMock.cs
// LAYER: Infrastructure (Security)
// WHAT IS THIS?
// Simulates Azure KeyVault for local development.
// In production, you would use Microsoft.Extensions.Configuration.AzureKeyVault.
// ==================================================================

using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace FinVault.Shared.Security;

public static class KeyVaultConfigurationSource
{
    public static IConfigurationBuilder AddMockKeyVault(this IConfigurationBuilder builder)
    {
        // Simulate fetching secrets from a secure vault
        // Note: connection string is intentionally left out here so the
        // value from appsettings.json or environment variables takes precedence
        var secrets = new Dictionary<string, string?>
        {
            { "Jwt:Secret", "FinVault-Super-Secret-Key-Must-Be-32-Chars!!" }
        };

        return builder.AddInMemoryCollection(secrets);
    }
}
