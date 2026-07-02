using RaidersVault.Models;

namespace RaidersVault.ViewModels;

public class BlueprintIndexViewModel
{
    public string? SearchTerm { get; set; }

    public string Playstyle { get; set; } = "Balanced";

    public List<Blueprint> Blueprints { get; set; } = new();

    public Dictionary<int, BlueprintFarmPlanViewModel> FarmPlans { get; set; } = new();

    public Dictionary<int, string> IconPaths { get; set; } = new();

    public bool HasBlueprints =>
        Blueprints.Any();

    public bool HasFarmPlans =>
        FarmPlans.Any();
}
