namespace RaidersVault.Models;

public class MapConditionOption
{
    public int Id { get; set; }

    public string MapName { get; set; } = string.Empty;

    public string ConditionName { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}
