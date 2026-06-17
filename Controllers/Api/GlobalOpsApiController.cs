using Microsoft.AspNetCore.Mvc;
using RaidersVault.Services;

namespace RaidersVault.Controllers.Api;

[ApiController]
[Route("api/v1/global-ops")]
public class GlobalOpsApiController : ControllerBase
{
    private readonly GlobalOpsService _globalOpsService;
    private readonly AuditService _auditService;

    public GlobalOpsApiController(
        GlobalOpsService globalOpsService,
        AuditService auditService)
    {
        _globalOpsService = globalOpsService;
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        if (HttpContext.Session.GetString("User") == null)
        {
            return Unauthorized(new
            {
                message = "Authentication is required to access Global Ops intelligence."
            });
        }

        var dashboard = await _globalOpsService.BuildDashboardAsync();
        await _auditService.RecordAsync(
            "API Read",
            HttpContext.Session.GetString("User") ?? "unknown",
            "Global Ops API",
            "Authenticated client requested Global Ops JSON intelligence.");

        return Ok(new
        {
            generatedAtUtc = dashboard.GeneratedAtUtc,
            readiness = new
            {
                tier = dashboard.ReadinessTier,
                overallPercent = dashboard.OverallReadinessPercent,
                questPercent = dashboard.QuestCompletionPercent,
                blueprintPercent = dashboard.BlueprintCompletionPercent,
                inventoryPercent = dashboard.InventoryReadinessPercent,
                activeRiskSignals = dashboard.ActiveRiskSignals
            },
            profile = new
            {
                dashboard.PlayerName,
                dashboard.PreferredPlaystyle,
                dashboard.DefaultMap
            },
            regions = dashboard.Regions,
            marketplace = dashboard.MarketSignals,
            neededItems = dashboard.NeededItems,
            blueprintTargets = dashboard.BlueprintTargets,
            weeklyTrials = dashboard.TrialSignals,
            liveOps = dashboard.LiveOps,
            localization = dashboard.LocalizationSignals,
            capabilities = dashboard.Capabilities
        });
    }
}
