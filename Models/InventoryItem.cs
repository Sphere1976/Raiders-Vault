using System.ComponentModel.DataAnnotations;

namespace RaidersVault.Models;

public class InventoryItem : TrackedRecord
{
    [Required, StringLength(60)] public string Category { get; set; } = "Material";
    [StringLength(40)] public string Rarity { get; set; } = "Common";
    [StringLength(100)] public string BestSource { get; set; } = "Any Map";
    [StringLength(100)] public string UsedFor { get; set; } = "Crafting";
    public int KeepTarget { get; set; } = 5;
    public int CurrentCount { get; set; }
    public int SellValue { get; set; }
    public bool Favorite { get; set; }
    public int Needed => Math.Max(KeepTarget - CurrentCount, 0);
    public string Priority => Needed == 0 ? "Stocked" : Rarity is "Legendary" or "Epic" ? "Critical" : Needed >= 5 ? "High" : "Medium";
}
