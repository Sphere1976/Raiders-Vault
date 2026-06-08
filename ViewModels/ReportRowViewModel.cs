namespace RaidersVault.ViewModels;

public class ReportRowViewModel
{
    public string RecordType { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string DetailOne { get; set; } = string.Empty;

    public DateTime LastUpdated { get; set; }

    public string DetailTwo { get; set; } = string.Empty;

    public int RecordId { get; set; }

    public string ControllerName { get; set; } = string.Empty;

    public string ActionName { get; set; } = "Details";

    public bool HasSecondaryDetail =>
        !string.IsNullOrWhiteSpace(DetailTwo);

    public ReportRowViewModel()
    {
    }

    public ReportRowViewModel(
        string recordType,
        string name,
        string status,
        string detailOne,
        DateTime lastUpdated,
        string detailTwo,
        int recordId)
    {
        RecordType = recordType;
        Name = name;
        Status = status;
        DetailOne = detailOne;
        LastUpdated = lastUpdated;
        DetailTwo = detailTwo;
        RecordId = recordId;

        ControllerName = recordType switch
        {
            "Loadout" => "Loadouts",
            "Quest" => "Quests",
            "Blueprint" => "Blueprints",
            _ => "Home"
        };
    }
}