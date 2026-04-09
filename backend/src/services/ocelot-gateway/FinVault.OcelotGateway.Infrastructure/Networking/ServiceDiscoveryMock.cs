// ==================================================================
// FILE : ServiceDiscoveryMock.cs
// LAYER: Infrastructure (Networking)
// WHAT IS THIS?
// Simulates a Service Discovery tool like Consul or Eureka.
// It maps Service Names to their current URLs.
// ==================================================================

using System.Collections.Generic;

namespace FinVault.Shared.Networking;

public static class ServiceDiscovery
{
    private static readonly Dictionary<string, string> _registry = new()
    {
        { "identity-service", "http://localhost:5232" },
        { "card-service",     "http://localhost:5121" },
        { "billing-service",  "http://localhost:5161" },
        { "payment-service",  "http://localhost:5181" },
        { "notification-service", "http://localhost:5191" },
        { "audit-service",    "http://localhost:5251" }
    };

    public static string GetServiceUrl(string serviceName)
    {
        return _registry.TryGetValue(serviceName, out var url) ? url : string.Empty;
    }
}
