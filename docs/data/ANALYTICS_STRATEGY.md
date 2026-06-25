# Analytics Strategy

Raiders Vault can evolve from an operational app into an analytics platform by publishing facts from
MVC workflows, inventory snapshots, audit events, and live condition refreshes into a warehouse.

## Data Products

- Inventory readiness by rarity and date
- Blueprint collection progress
- Global Ops usage and export history
- Live condition frequency and response windows
- Regional support and localization readiness

## Warehouse Artifacts

- `infra/data-warehouse/schema.sql`
- `infra/data-warehouse/analytics_views.sql`

## Event Sources

- `AuditEventRecorded`
- `GlobalOpsExported`
- `InventorySnapshotCaptured`
- `LiveOpsSnapshotRefreshed`
- `BlueprintFarmPlanGenerated`

## Governance

- Use non-sensitive operational facts only.
- Avoid storing passwords, session tokens, or private notes.
- Add data-quality checks before promoting warehouse views to dashboards.
- Version schema migrations with release notes.
