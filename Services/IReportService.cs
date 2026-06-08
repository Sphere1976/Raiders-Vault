using RaidersVault.ViewModels;

namespace RaidersVault.Services;

public interface IReportService
{
    ReportViewModel BuildSummaryReport(
        string recordType = "All",
        string statusFilter = "All",
        string searchTerm = "");
}