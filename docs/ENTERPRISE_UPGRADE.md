# Raiders Vault Enterprise Upgrade Plan

This document outlines how Raiders Vault can be evolved from a strong portfolio application into an enterprise-grade SaaS-style product.

## Current Enterprise Improvements Added

- Rebranded application shell from companion-project language to an operations intelligence platform.
- Replaced school-focused README with a product and engineering README.
- Added security headers middleware.
- Added health check endpoint at `/health`.
- Enabled response compression.
- Hardened session cookie behavior for deployed environments.
- Added professional documentation structure.
- Added CI workflow scaffold.
- Cleaned repository package by excluding `.git`, `.vs`, `bin`, and `obj` artifacts from the delivered zip.

## Priority 1 — Identity and Access Management

Replace the simple seeded login with a production identity system.

Recommended options:

- ASP.NET Core Identity
- Auth0
- Microsoft Entra ID
- Google OAuth

Add:

- Password reset
- Account lockout
- Email confirmation
- Role-based authorization
- Admin/operator separation
- User profile ownership on records

## Priority 2 — Database Modernization

SQLite is good for local and demo use, but enterprise deployment should use a managed relational database.

Recommended targets:

- PostgreSQL on Cloud SQL
- SQL Server on Azure SQL
- MySQL on Cloud SQL

Add:

- EF Core migrations
- Connection pooling
- Backup/restore strategy
- Data retention policy
- Migration scripts for production releases

## Priority 3 — API Layer

Add REST endpoints for core resources.

Suggested API areas:

- `/api/loadouts`
- `/api/objectives`
- `/api/blueprints`
- `/api/run-plans`
- `/api/reports`

Add:

- Swagger/OpenAPI
- DTOs
- API versioning
- Request validation
- Rate limiting
- API integration tests

## Priority 4 — Observability

Add real operational monitoring.

Recommended controls:

- Structured logging
- Correlation IDs
- Request duration logging
- Error tracking
- Health probes
- Readiness probes
- Dashboard metrics

Suggested tools:

- Serilog
- OpenTelemetry
- Application Insights
- Google Cloud Logging
- Prometheus/Grafana

## Priority 5 — Enterprise UI/UX

Current UI is now portfolio-grade. To move toward enterprise SaaS quality, add:

- Sidebar layout
- User avatar menu
- Global search
- Command palette
- Toast notifications
- Empty states
- Loading states
- Confirmation modals
- Data tables with sorting and pagination
- Export controls
- Accessible color contrast review

## Priority 6 — Testing and Quality Gates

Add:

- Unit tests for services
- Controller tests
- Integration tests
- Playwright UI tests
- Code coverage reporting
- Static analysis
- Dependency vulnerability scanning

Minimum release gate:

```text
Build passes
Tests pass
No critical vulnerabilities
No formatting errors
Docker image builds
Health endpoint responds
```

## Priority 7 — Product Features

Potential enterprise-grade product features:

- Multi-user accounts
- Shared team workspaces
- Cloud-synced watchlists
- Personal and team dashboards
- Advanced analytics
- Export to CSV/PDF
- Saved report filters
- Notification rules
- Real-time event data integrations
- rules-based run recommendations

## Recommended Next Implementation Sprint

1. Add ASP.NET Core Identity.
2. Add PostgreSQL support.
3. Add migrations.
4. Add audit logging.
5. Add admin dashboard.
6. Add Swagger API.
7. Add integration tests.
8. Deploy to Cloud Run with managed database.

## MetaForge-Inspired Professional Companion Upgrade

This update expands Raiders Vault from a basic CRUD planner into a more polished companion-style command platform. The design is intentionally inspired by modern game companion hubs such as MetaForge, which are known for combining guides, tools, interactive map-style planning, item intelligence, overlays, and event alerts into one workflow.

### Added polish

- Rebuilt the Blueprint Tracker into a card-based command vault with status badges, collection metrics, farm-plan previews, best map, best condition, and relative farming weight.
- Expanded the Run Planner with a MetaForge-style intelligence layer that keeps map, condition, target, route, and checklist decisions visible together.
- Added generated mission risk level, priority score, extraction window, route sequence, threat controls, and operator tips.
- Improved the user experience by replacing plain blueprint rows with scannable professional cards and faster action buttons.
- Kept all logic local to the application so the project remains evaluator-friendly and does not require third-party API credentials.
- Added a Global Ops center for worldwide regional readiness, local-time planning windows, marketplace-style value signals, needed-item prioritization, and enterprise capability status.
- Added a reusable Global Ops service so MVC, CSV export, and API integrations use the same operational-readiness calculations.
- Added `/GlobalOps/ExportCsv` for exporting marketplace, needed item, blueprint, trial, and region signals.
- Added `/api/v1/global-ops` as a versioned JSON endpoint for future overlay, companion app, mobile, or external dashboard integrations.
- Added localization-readiness cards for regional language, currency, and support-window planning.
- Added an Admin Center for security posture, integration registry, environment health, data coverage, and recent audit activity.
- Added persistent audit events for login, logout, Global Ops CSV export, and authenticated Global Ops API access.
- Added database repair and seed support for audit history so existing demo databases upgrade without manual resets.
- Imported the full MetaForge ARC Raiders item database from `/arc-raiders/database/items/page/1` through `/page/15`, adding 581 item records to the local Item Database while preserving existing inventory counts.
- Added an Embark Live Ops feed with official ARC Raiders news cards, active map-condition banners, upcoming condition rotations, and protected API exposure.

### Why this helps the capstone

The program now presents more like a production planning dashboard than a school CRUD app. It demonstrates stronger UI/UX design, decision support, data-driven recommendations, operational workflow, and professional documentation while preserving the original ASP.NET Core MVC architecture.
