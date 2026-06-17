using RaidersVault.Models;

namespace RaidersVault.ViewModels;

public class AdminCenterViewModel
{
    public int UserCount { get; set; }

    public int LoadoutCount { get; set; }

    public int ObjectiveCount { get; set; }

    public int BlueprintCount { get; set; }

    public int InventoryCount { get; set; }

    public int ApiCapabilityCount { get; set; }

    public int CriticalAuditCount { get; set; }

    public int SecurityScore { get; set; }

    public string EnvironmentName { get; set; } = "Development";

    public string DatabaseProvider { get; set; } = "SQLite";

    public DateTimeOffset GeneratedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public List<AdminCapabilityStatus> CapabilityStatuses { get; set; } = new();

    public List<AdminIntegrationEndpoint> IntegrationEndpoints { get; set; } = new();

    public List<AuditEvent> RecentAuditEvents { get; set; } = new();
}

public class AdminCapabilityStatus
{
    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = "Ready";

    public string Detail { get; set; } = string.Empty;
}

public class AdminIntegrationEndpoint
{
    public string Name { get; set; } = string.Empty;

    public string Method { get; set; } = "GET";

    public string Path { get; set; } = string.Empty;

    public string Access { get; set; } = "Session";
}
