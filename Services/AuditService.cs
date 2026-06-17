using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;
using RaidersVault.Models;

namespace RaidersVault.Services;

public class AuditService
{
    private readonly RaidersVaultContext _db;

    public AuditService(RaidersVaultContext db)
    {
        _db = db;
    }

    public async Task RecordAsync(
        string eventType,
        string actor,
        string area,
        string details,
        string severity = "Info")
    {
        var now = DateTime.UtcNow;

        _db.AuditEvents.Add(new AuditEvent
        {
            Name = $"{area}: {eventType}",
            EventType = eventType,
            Actor = string.IsNullOrWhiteSpace(actor) ? "unknown" : actor,
            Area = area,
            Severity = severity,
            Details = details,
            OccurredAt = now,
            CreatedAt = now,
            UpdatedAt = now,
            Notes = details
        });

        await _db.SaveChangesAsync();
    }

    public async Task<List<AuditEvent>> RecentAsync(int take = 20)
    {
        return await _db.AuditEvents
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id)
            .Take(take)
            .ToListAsync();
    }
}
