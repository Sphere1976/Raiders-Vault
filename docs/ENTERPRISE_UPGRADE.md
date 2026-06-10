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
