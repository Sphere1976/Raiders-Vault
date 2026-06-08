using Microsoft.AspNetCore.Mvc;

namespace RaidersVault.Controllers;

public abstract class BaseController : Controller
{
    protected bool IsLoggedIn()
    {
        return HttpContext.Session.GetString("User") != null;
    }

    protected IActionResult RequireLogin()
    {
        return RedirectToAction("Login", "Account");
    }
}