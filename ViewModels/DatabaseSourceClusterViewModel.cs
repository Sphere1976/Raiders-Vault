namespace RaidersVault.ViewModels;

public sealed class DatabaseSourceClusterViewModel
{
    public string Source { get; set; } = "Unknown Source";

    public int ItemCount { get; set; }

    public int NeededUnits { get; set; }

    public int CriticalCount { get; set; }

    public int TotalValue { get; set; }

    public string TopItemName { get; set; } = "No target";

    public string Recommendation { get; set; } = "No route recommendation available.";
}
