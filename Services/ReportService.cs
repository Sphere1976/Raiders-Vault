using RaidersVault.Data;
using RaidersVault.Models;
using RaidersVault.ViewModels;

namespace RaidersVault.Services;

public class ReportService : IReportService
{
    private readonly RaidersVaultContext _context;

    public ReportService(RaidersVaultContext context)
    {
        _context = context;
    }

    public ReportViewModel BuildSummaryReport(
        string recordType = "All",
        string statusFilter = "All",
        string searchTerm = "")
    {
        recordType = string.IsNullOrWhiteSpace(recordType)
            ? "All"
            : recordType;

        statusFilter = string.IsNullOrWhiteSpace(statusFilter)
            ? "All"
            : statusFilter;

        searchTerm = searchTerm?.Trim() ?? string.Empty;

        var rows = new List<ReportRowViewModel>();

        var loadouts = _context.Loadouts
            .OrderBy(x => x.Name)
            .ToList();

        var quests = _context.Quests
            .OrderBy(x => x.Name)
            .ToList();

        var blueprints = _context.Blueprints
            .OrderBy(x => x.Name)
            .ToList();

        if (recordType == "All" || recordType == "Loadout")
        {
            foreach (var item in loadouts)
            {
                rows.Add(new ReportRowViewModel(
                    "Loadout",
                    item.Name,
                    item.ActivityType,
                    item.MapOrEvent,
                    item.UpdatedAt,
                    "Loadout planning record",
                    item.Id));
            }
        }

        if (recordType == "All" || recordType == "Quest")
        {
            foreach (var item in quests)
            {
                var status = IsQuestComplete(item)
                    ? "Complete"
                    : NormalizeOpenQuestStatus(item.Status);

                rows.Add(new ReportRowViewModel(
                    "Quest",
                    item.Name,
                    status,
                    item.Priority,
                    item.UpdatedAt,
                    item.RelatedActivity ?? string.Empty,
                    item.Id));
            }
        }

        if (recordType == "All" || recordType == "Blueprint")
        {
            foreach (var item in blueprints)
            {
                var status = IsBlueprintCollected(item)
                    ? "Collected"
                    : NormalizeOpenBlueprintStatus(item.CollectionStatus);

                rows.Add(new ReportRowViewModel(
                    "Blueprint",
                    item.Name,
                    status,
                    item.Category,
                    item.UpdatedAt,
                    item.WhereToGet ?? string.Empty,
                    item.Id));
            }
        }

        if (statusFilter == "Completed")
        {
            rows = rows
                .Where(x => IsCompletedReportStatus(x.Status))
                .ToList();
        }
        else if (statusFilter == "Open")
        {
            rows = rows
                .Where(x => !IsCompletedReportStatus(x.Status))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            rows = rows.Where(x =>
                    x.RecordType.Contains(
                        searchTerm,
                        StringComparison.OrdinalIgnoreCase)
                    || x.Name.Contains(
                        searchTerm,
                        StringComparison.OrdinalIgnoreCase)
                    || x.Status.Contains(
                        searchTerm,
                        StringComparison.OrdinalIgnoreCase)
                    || x.DetailOne.Contains(
                        searchTerm,
                        StringComparison.OrdinalIgnoreCase)
                    || x.DetailTwo.Contains(
                        searchTerm,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return new ReportViewModel
        {
            Title = "Raiders Vault Planning Summary Report",

            GeneratedAt = DateTime.Now,

            SelectedRecordType = recordType,

            SelectedStatusFilter = statusFilter,

            SearchTerm = searchTerm,

            TotalLoadouts = loadouts.Count,

            TotalQuests = quests.Count,

            CompletedQuests = quests.Count(IsQuestComplete),

            OpenQuests = quests.Count(x => !IsQuestComplete(x)),

            TotalBlueprints = blueprints.Count,

            CollectedBlueprints = blueprints.Count(IsBlueprintCollected),

            MissingBlueprints = blueprints.Count(x => !IsBlueprintCollected(x)),

            FavoriteLoadouts = 0,

            Rows = rows
                .OrderBy(x => x.RecordType)
                .ThenBy(x => x.Name)
                .ToList()
        };
    }

    private static bool IsQuestComplete(Quest quest)
    {
        return string.Equals(
                   quest.Status,
                   "Complete",
                   StringComparison.OrdinalIgnoreCase)
               || string.Equals(
                   quest.Status,
                   "Completed",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBlueprintCollected(Blueprint blueprint)
    {
        return blueprint.Collected
               || string.Equals(
                   blueprint.CollectionStatus,
                   "Collected",
                   StringComparison.OrdinalIgnoreCase)
               || string.Equals(
                   blueprint.CollectionStatus,
                   "Obtained",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCompletedReportStatus(string? status)
    {
        return string.Equals(
                   status,
                   "Complete",
                   StringComparison.OrdinalIgnoreCase)
               || string.Equals(
                   status,
                   "Completed",
                   StringComparison.OrdinalIgnoreCase)
               || string.Equals(
                   status,
                   "Collected",
                   StringComparison.OrdinalIgnoreCase)
               || string.Equals(
                   status,
                   "Obtained",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeOpenQuestStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "In Progress";
        }

        if (string.Equals(
            status,
            "Tracking",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Tracking";
        }

        return "In Progress";
    }

    private static string NormalizeOpenBlueprintStatus(string? status)
    {
        if (string.Equals(
            status,
            "Tracking",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Tracking";
        }

        return "Not Collected";
    }
}