using Microsoft.AspNetCore.Mvc;
using RaidersVault.Data;
using RaidersVault.Services;
using System.Security.Cryptography;
using System.Text;

namespace RaidersVault.Controllers;

public class AccountController : Controller
{
    private readonly RaidersVaultContext _context;
    private readonly AuditService _auditService;

    public AccountController(
        RaidersVaultContext context,
        AuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        username = InputSanitizer.Clean(username);
        password ??= string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Username and password are required.";
            return View();
        }

        var user = _context.UserAccounts.FirstOrDefault(x => x.Username == username);

        if (user == null || !PasswordMatches(password, user.PasswordHash))
        {
            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        HttpContext.Session.SetString("User", user.Username);
        await _auditService.RecordAsync(
            "Login",
            user.Username,
            "Identity",
            "User signed in to the enterprise command platform.");

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        var actor = HttpContext.Session.GetString("User") ?? "unknown";
        await _auditService.RecordAsync(
            "Logout",
            actor,
            "Identity",
            "User signed out of the platform.");

        HttpContext.Session.Clear();

        return RedirectToAction("Login");
    }

    private static bool PasswordMatches(string password, string storedHash)
    {
        var enteredHash = RaidersVault.Services.DbInitializer.HashPassword(password);
        var enteredBytes = Encoding.UTF8.GetBytes(enteredHash);
        var storedBytes = Encoding.UTF8.GetBytes(storedHash);

        return enteredBytes.Length == storedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(enteredBytes, storedBytes);
    }
}
