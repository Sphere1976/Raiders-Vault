using System.Text;
using Microsoft.AspNetCore.Mvc;
using RaidersVault.Services;

namespace RaidersVault.Controllers;

public class GlobalOpsController : BaseController
{
    private readonly GlobalOpsService _globalOpsService;
    private readonly AuditService _auditService;

    public GlobalOpsController(
        GlobalOpsService globalOpsService,
        AuditService auditService)
    {
        _globalOpsService = globalOpsService;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        return View(await _globalOpsService.BuildDashboardAsync());
    }

    public async Task<IActionResult> ExportCsv()
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var dashboard = await _globalOpsService.BuildDashboardAsync();
        var csv = _globalOpsService.BuildCsv(dashboard);
        var fileName = $"raiders-vault-global-ops-{DateTime.UtcNow:yyyyMMdd-HHmm}.csv";
        await _auditService.RecordAsync(
            "CSV Export",
            HttpContext.Session.GetString("User") ?? "unknown",
            "Global Ops",
            "Global Ops intelligence was exported as CSV.");

        return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
    }
}
