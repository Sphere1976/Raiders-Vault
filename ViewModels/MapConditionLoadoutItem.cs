namespace RaidersVault.ViewModels;

public class MapConditionLoadoutItem
{
    public string Slot { get; set; } = string.Empty;

    public string Recommendation { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public bool HasReason =>
        !string.IsNullOrWhiteSpace(Reason);
}