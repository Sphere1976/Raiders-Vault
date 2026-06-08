using Microsoft.AspNetCore.Mvc;
using RaidersVault.Services;

namespace RaidersVault.Controllers;

public class ReportsController : BaseController
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    public IActionResult Index(
        string recordType = "All",
        string statusFilter = "All",
        string searchTerm = "")
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var report = _reportService.BuildSummaryReport(
            recordType,
            statusFilter,
            searchTerm);

        return View(report);
    }
}