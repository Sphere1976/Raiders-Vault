namespace RaidersVault.ViewModels;

public class ArcRaidersLiveOpsViewModel
{
    public DateTimeOffset GeneratedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public string SourceName { get; set; } = "Official ARC Raiders";

    public string SourceUrl { get; set; } = "https://arcraiders.com/";

    public List<LiveMapConditionItem> ActiveConditions { get; set; } = new();

    public List<LiveMapConditionItem> UpcomingConditions { get; set; } = new();

    public List<EmbarkNewsItem> NewsItems { get; set; } = new();
}

public class LiveMapConditionItem
{
    public string ConditionName { get; set; } = string.Empty;

    public string MapName { get; set; } = string.Empty;

    public string Window { get; set; } = string.Empty;

    public string TimeRemaining { get; set; } = string.Empty;

    public string Status { get; set; } = "Active";

    public string SourceUrl { get; set; } = "https://arcraiders.com/map-conditions";
}

public class EmbarkNewsItem
{
    public string Category { get; set; } = "Update";

    public string Title { get; set; } = string.Empty;

    public string PublishedAt { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Url { get; set; } = "https://arcraiders.com/news";
}
