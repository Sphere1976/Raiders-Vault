using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;
using RaidersVault.Middleware;
using RaidersVault.Services;

var builder = WebApplication.CreateBuilder(args);
var keyRingPath = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");

LoadLocalEnvironment(builder.Environment.ContentRootPath);
Directory.CreateDirectory(keyRingPath);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
    .SetApplicationName("RaidersVault");

builder.Services.AddDbContext<RaidersVaultContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<LoadoutRecommendationService>();
builder.Services.AddScoped<BlueprintRecommendationService>();
builder.Services.AddScoped<GlobalOpsService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddHttpClient<AiChatService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(18);
});
builder.Services.AddHttpClient<ArcRaidersLiveOpsService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(4);
});
builder.Services.AddHealthChecks();
builder.Services.AddResponseCompression();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.Name = "RaidersVault.Session";
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RaidersVaultContext>();

    db.Database.EnsureCreated();
    DatabaseRepairService.EnsurePortfolioTables(db);
    DbInitializer.Seed(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

static void LoadLocalEnvironment(string contentRootPath)
{
    var envPath = Path.Combine(contentRootPath, ".env.local");
    if (!File.Exists(envPath))
    {
        return;
    }

    foreach (var rawLine in File.ReadAllLines(envPath))
    {
        var line = rawLine.Trim();
        if (line.Length == 0 || line.StartsWith('#'))
        {
            continue;
        }

        var separatorIndex = line.IndexOf('=');
        if (separatorIndex <= 0)
        {
            continue;
        }

        var name = line[..separatorIndex].Trim();
        var value = line[(separatorIndex + 1)..].Trim().Trim('"');
        if (Environment.GetEnvironmentVariable(name) == null)
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }
}

public partial class Program
{
}
