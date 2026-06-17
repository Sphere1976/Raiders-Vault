using System.ComponentModel.DataAnnotations;

namespace RaidersVault.Models;

public class AuditEvent : TrackedRecord
{
    [Required, StringLength(60)]
    public string EventType { get; set; } = "System";

    [Required, StringLength(80)]
    public string Actor { get; set; } = "system";

    [Required, StringLength(80)]
    public string Area { get; set; } = "Platform";

    [Required, StringLength(40)]
    public string Severity { get; set; } = "Info";

    [StringLength(500)]
    public string Details { get; set; } = string.Empty;

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
