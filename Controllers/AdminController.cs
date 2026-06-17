using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;
using RaidersVault.Services;
using RaidersVault.ViewModels;

namespace RaidersVault.Controllers;

public class AdminController : BaseController
{
    private readonly RaidersVaultContext _db;
    private readonly AuditService _auditService;
    private readonly IWebHostEnvironment _environment;

    public AdminController(
        RaidersVaultContext db,
        AuditService auditService,
        IWebHostEnvironment environment)
    {
        _db = db;
        _auditService = auditService;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        var auditEvents = await _auditService.RecentAsync();

        var viewModel = new AdminCenterViewModel
        {
            UserCount = await _db.UserAccounts.CountAsync(),
            LoadoutCount = await _db.Loadouts.CountAsync(),
            ObjectiveCount = await _db.Quests.CountAsync(),
            BlueprintCount = await _db.Blueprints.CountAsync(),
            InventoryCount = await _db.InventoryItems.CountAsync(),
            ApiCapabilityCount = 3,
            CriticalAuditCount = auditEvents.Count(x => x.Severity == "Critical" || x.Severity == "Warning"),
            SecurityScore = BuildSecurityScore(auditEvents),
            EnvironmentName = _environment.EnvironmentName,
            DatabaseProvider = _db.Database.ProviderName ?? "Unknown",
            RecentAuditEvents = auditEvents,
            CapabilityStatuses = BuildCapabilities(),
            IntegrationEndpoints = BuildIntegrations()
        };

        return View(viewModel);
    }

    private static int BuildSecurityScore(IEnumerable<Models.AuditEvent> auditEvents)
    {
        var warnings = auditEvents.Count(x => x.Severity == "Warning");
        var critical = auditEvents.Count(x => x.Severity == "Critical");

        return Math.Max(65, 100 - warnings * 4 - critical * 12);
    }

    private static List<AdminCapabilityStatus> BuildCapabilities()
    {
        return new List<AdminCapabilityStatus>
        {
            new() { Name = "Session security", Status = "Live", Detail = "HttpOnly, SameSite, secure production cookie policy, and server-side session checks." },
            new() { Name = "Data protection", Status = "Live", Detail = "Application key ring is persisted under the app root for stable auth/session cryptography." },
            new() { Name = "Security headers", Status = "Live", Detail = "Central middleware applies hardened browser and transport security headers." },
            new() { Name = "Operational health", Status = "Live", Detail = "Health endpoint supports uptime checks and deployment smoke tests." },
            new() { Name = "Audit trail", Status = "Live", Detail = "Authentication, export, and API access events are captured for governance review." },
            new() { Name = "Enterprise API", Status = "Protected", Detail = "Global Ops JSON intelligence requires an authenticated session." }
        };
    }

    private static List<AdminIntegrationEndpoint> BuildIntegrations()
    {
        return new List<AdminIntegrationEndpoint>
        {
            new() { Name = "Health Probe", Method = "GET", Path = "/health", Access = "Public" },
            new() { Name = "Global Ops API", Method = "GET", Path = "/api/v1/global-ops", Access = "Session" },
            new() { Name = "Global Ops CSV", Method = "GET", Path = "/GlobalOps/ExportCsv", Access = "Session" },
            new() { Name = "Analytics Report", Method = "GET", Path = "/Reports", Access = "Session" }
        };
    }
}
