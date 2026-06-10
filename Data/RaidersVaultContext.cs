using Microsoft.EntityFrameworkCore;
using RaidersVault.Models;

namespace RaidersVault.Data;

public class RaidersVaultContext : DbContext
{
    public RaidersVaultContext(
        DbContextOptions<RaidersVaultContext> options)
        : base(options)
    {
    }

    public DbSet<Loadout> Loadouts => Set<Loadout>();

    public DbSet<Quest> Quests => Set<Quest>();

    public DbSet<Blueprint> Blueprints => Set<Blueprint>();

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<UserSkill> UserSkills => Set<UserSkill>();

    public DbSet<PlayerProfile> PlayerProfiles => Set<PlayerProfile>();

    public DbSet<FavoriteItem> FavoriteItems => Set<FavoriteItem>();

    public DbSet<RivenTidesRecord> RivenTidesRecords => Set<RivenTidesRecord>();

    public DbSet<MapConditionOption> MapConditionOptions => Set<MapConditionOption>();

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<IntelGuide> IntelGuides => Set<IntelGuide>();
    public DbSet<WeeklyTrial> WeeklyTrials => Set<WeeklyTrial>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserSkill>()
            .HasIndex(x => x.SkillId)
            .IsUnique();

        modelBuilder.Entity<FavoriteItem>()
            .HasIndex(x => new
            {
                x.ItemType,
                x.ItemId
            })
            .IsUnique();

        modelBuilder.Entity<MapConditionOption>()
            .HasIndex(x => new
            {
                x.MapName,
                x.ConditionName
            })
            .IsUnique();

        modelBuilder.Entity<InventoryItem>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<IntelGuide>().HasIndex(x => new { x.Name, x.MapName, x.MapCondition }).IsUnique();
        modelBuilder.Entity<WeeklyTrial>().HasIndex(x => x.Name).IsUnique();
    }
}