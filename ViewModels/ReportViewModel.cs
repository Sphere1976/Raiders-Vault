namespace RaidersVault.ViewModels;

public class ReportViewModel
{
    public string Title { get; set; } =
        "Raiders Vault Planning Summary Report";

    public DateTime GeneratedAt { get; set; } = DateTime.Now;

    public string SelectedRecordType { get; set; } = "All";

    public string SelectedStatusFilter { get; set; } = "All";

    public string SearchTerm { get; set; } = string.Empty;

    public int TotalLoadouts { get; set; }

    public int TotalQuests { get; set; }

    public int CompletedQuests { get; set; }

    public int OpenQuests { get; set; }

    public int TotalBlueprints { get; set; }

    public int CollectedBlueprints { get; set; }

    public int MissingBlueprints { get; set; }

    public int FavoriteLoadouts { get; set; }

    public List<ReportRowViewModel> Rows { get; set; } = new();

    public bool HasRows => Rows.Any();
}