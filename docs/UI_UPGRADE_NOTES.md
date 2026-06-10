# Raiders Vault UI Upgrade Notes

This update turns the home page into a larger, brighter, more premium command-center dashboard while keeping the original ASP.NET Core MVC structure intact.

## Upgraded Areas

- Rebuilt the dashboard hero into a full enterprise command-center layout.
- Added high-impact metric cards for loadouts, pinned intel, quests, and blueprints.
- Added a modern module grid for Blueprint Tracker, Run Planner, Map Intelligence, Skill Builder, Objective Board, and Analytics Reports.
- Added a clearer tactical workflow section for evaluator-friendly explanation.
- Expanded the global CSS visual system with stronger gradients, neon accents, glass panels, responsive grids, premium buttons, and improved spacing.

## Inspiration

The application was visually upgraded toward the feel of modern game companion hubs and ARC Raiders resource sites, while keeping the code original and focused on the existing Raiders Vault features.

## Technical Notes

- Files changed:
  - `Views/Home/Index.cshtml`
  - `wwwroot/css/site.css`
  - `docs/UI_UPGRADE_NOTES.md`
- No database schema changes.
- No controller changes.
- No model changes.
- No package changes.
- Existing ViewBag values are still used for live dashboard counts.

## Build Note

Run locally with:

```powershell
dotnet clean
dotnet restore
dotnet build -c Release
dotnet run
```
