using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;

namespace RaidersVault.Controllers;

public class TrialsController : BaseController
{
    private readonly RaidersVaultContext _db;
    public TrialsController(RaidersVaultContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        if (!IsLoggedIn())
        {
            return RequireLogin();
        }

        return View(await _db.WeeklyTrials
            .OrderBy(x => x.ObjectiveType)
            .ThenBy(x => x.Name)
            .ToListAsync());
    }
}
