using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;

namespace RaidersVault.Controllers;

public class IntelController : BaseController
{
    private readonly RaidersVaultContext _db;
    public IntelController(RaidersVaultContext db) => _db = db;
    public async Task<IActionResult> Index() { RequireLogin(); return View(await _db.IntelGuides.OrderBy(x => x.MapName).ToListAsync()); }
}
