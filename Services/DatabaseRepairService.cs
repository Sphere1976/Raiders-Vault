using Microsoft.EntityFrameworkCore;
using RaidersVault.Data;

namespace RaidersVault.Services;

/// <summary>
/// Keeps the local SQLite database compatible when portfolio/demo features are added
/// without requiring the evaluator to delete the existing database first.
/// </summary>
public static class DatabaseRepairService
{
    public static void EnsurePortfolioTables(RaidersVaultContext db)
    {
        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "InventoryItems" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_InventoryItems" PRIMARY KEY AUTOINCREMENT,
                "Name" TEXT NOT NULL,
                "Notes" TEXT NULL,
                "CreatedAt" TEXT NOT NULL,
                "UpdatedAt" TEXT NOT NULL,
                "Category" TEXT NOT NULL,
                "Rarity" TEXT NOT NULL,
                "BestSource" TEXT NOT NULL,
                "UsedFor" TEXT NOT NULL,
                "KeepTarget" INTEGER NOT NULL,
                "CurrentCount" INTEGER NOT NULL,
                "SellValue" INTEGER NOT NULL,
                "Favorite" INTEGER NOT NULL
            );
            """);

        db.Database.ExecuteSqlRaw("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_InventoryItems_Name"
            ON "InventoryItems" ("Name");
            """);

        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "IntelGuides" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_IntelGuides" PRIMARY KEY AUTOINCREMENT,
                "Name" TEXT NOT NULL,
                "Notes" TEXT NULL,
                "CreatedAt" TEXT NOT NULL,
                "UpdatedAt" TEXT NOT NULL,
                "GuideType" TEXT NOT NULL,
                "MapName" TEXT NOT NULL,
                "MapCondition" TEXT NOT NULL,
                "Difficulty" TEXT NOT NULL,
                "RecommendedRoute" TEXT NOT NULL,
                "LootFocus" TEXT NOT NULL,
                "RiskWarning" TEXT NOT NULL
            );
            """);

        db.Database.ExecuteSqlRaw("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_IntelGuides_Name_MapName_MapCondition"
            ON "IntelGuides" ("Name", "MapName", "MapCondition");
            """);

        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "WeeklyTrials" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_WeeklyTrials" PRIMARY KEY AUTOINCREMENT,
                "Name" TEXT NOT NULL,
                "Notes" TEXT NULL,
                "CreatedAt" TEXT NOT NULL,
                "UpdatedAt" TEXT NOT NULL,
                "ObjectiveType" TEXT NOT NULL,
                "TargetScore" INTEGER NOT NULL,
                "ScorePerAction" INTEGER NOT NULL,
                "BestMap" TEXT NOT NULL,
                "Strategy" TEXT NOT NULL
            );
            """);

        db.Database.ExecuteSqlRaw("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WeeklyTrials_Name"
            ON "WeeklyTrials" ("Name");
            """);
    }
}
