# Raiders Vault

> Enterprise-style operations intelligence platform for ARC Raiders planning, progression tracking, equipment strategy, and collection analytics.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-5C2D91?style=for-the-badge&logo=dotnet)
![SQLite](https://img.shields.io/badge/SQLite-Database-003B57?style=for-the-badge&logo=sqlite)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker)
![Google Cloud](https://img.shields.io/badge/Google_Cloud-Run-4285F4?style=for-the-badge&logo=googlecloud)

---

## Executive Overview

Raiders Vault is a full-stack ASP.NET Core MVC application that turns fragmented gameplay planning into a centralized operational command center. It combines loadout management, objective tracking, blueprint analytics, route planning, map-condition intelligence, skill progression, watchlists, and reporting into one cohesive platform.

This version is positioned as a production-quality portfolio application: clean branding, secure defaults, deployment readiness, technical documentation, and an enterprise-style product structure.

---

## Latest Enhancement Pass

This build expands the main planning workflow with a premium Run Planner command interface, mission briefing cards, route-aligned KPI tiles, a browser-local pre-extraction checklist, stacked blueprint intel cards, and stronger dashboard visual hierarchy. The goal is to make the application feel more like a polished operations product while keeping the existing ASP.NET Core MVC architecture intact.

---

## Product Capabilities

| Domain | Capability |
|---|---|
| Equipment Operations | Create, edit, score, search, and compare loadouts. |
| Objective Management | Track active, completed, and reopened objectives. |
| Blueprint Intelligence | Monitor collected and missing blueprints with farming context. |
| Route Planning | Generate farming strategies based on map, condition, and goal. |
| Map Intel | Evaluate active conditions and recommended loadout strategies. |
| Skill Planning | Build PvE, PvP, and balanced progression paths. |
| Watchlist | Pin high-value records for faster planning. |
| Reporting | Generate operational summaries across core data sets. |

---

## Architecture

```
    A[Browser Client] --> B[ASP.NET Core MVC]
    B --> C[Controllers]
    B --> D[Razor Views]
    C --> E[Application Services]
    E --> F[Entity Framework Core]
    F --> G[(SQLite Database)]
    B --> H[Session Authentication]
    B --> I[Security Headers Middleware]
    B --> J[Health Check Endpoint]
```

---

## Technology Stack

### Application

- ASP.NET Core 8 MVC
- C#
- Razor Views
- Entity Framework Core
- SQLite
- Dependency Injection
- Session-based authentication

### Production Readiness

- Dockerfile included
- Google Cloud Run compatible
- Health check endpoint: `/health`
- Response compression enabled
- Secure session cookie configuration
- Security headers middleware
- Anti-forgery validation enabled globally
- Environment-aware cookie security policy

### Security Controls

- Anti-forgery validation on MVC actions
- HttpOnly session cookies
- Strict SameSite cookie policy
- Secure cookies outside local development
- Fixed-time password hash comparison
- Input sanitization helper
- Content Security Policy
- X-Frame-Options
- X-Content-Type-Options
- Referrer Policy
- Permissions Policy

---

## Repository Structure

```
RaidersVault/
├── Controllers/              # MVC request handlers
├── Data/                     # EF Core context and data access
├── Middleware/               # Security and platform middleware
├── Models/                   # Domain entities
├── Services/                 # Business logic and recommendation services
├── ViewModels/               # UI-specific data contracts
├── Views/                    # Razor UI
├── wwwroot/                  # CSS, images, static assets
├── docs/                     # Architecture and product documentation
├── .github/workflows/        # CI pipeline
├── Dockerfile                # Container deployment
└── RaidersVault.csproj       # .NET project file
```

---

## Getting Started

### Requirements

- .NET 8 SDK
- Visual Studio 2022, VS Code, or JetBrains Rider

### Run Locally

```
dotnet restore
dotnet build
dotnet run
```

The SQLite database is created and seeded automatically on first launch.

### Default Development Login

```
Username: admin
Password: password
```

For public deployment, replace the seeded development account with an environment-driven identity strategy.

---

## Docker Deployment

```
docker build -t raiders-vault .
docker run -p 8080:8080 raiders-vault
```

---

## Google Cloud Run Deployment

```
gcloud run deploy raiders-vault \
  --source . \
  --region us-central1 \
  --platform managed \
  --allow-unauthenticated
```

---

## Screenshots

Add production screenshots under `wwwroot/images/portfolio/` and reference them here.

| Area | Screenshot |
|---|---|
| Command Center | `dashboard.png` |
| Run Planner | `run-planner.png` |
| Blueprint Intelligence | `blueprints.png` |
| Map Intel | `map-intel.png` |
| Analytics | `reports.png` |

---

## Enterprise Upgrade Roadmap

See [`docs/ENTERPRISE_UPGRADE.md`](docs/ENTERPRISE_UPGRADE.md) for the full modernization plan.

High-impact future upgrades:

- ASP.NET Core Identity or external OAuth provider
- PostgreSQL or SQL Server production database
- Role-based access control
- Audit logging
- API layer with OpenAPI/Swagger
- Automated tests and coverage gates
- Structured logging with correlation IDs
- CI/CD deployment pipeline
- Observability dashboards
- Multi-user cloud synchronization

---

## Author

Steven Buchholtz  
Full Stack Software Engineer  
ASP.NET Core • C# • EF Core • Cloud Deployment • Product Architecture

---

## License

Portfolio demonstration project.

---

## Production Portfolio Upgrade

This version is prepared for resume and portfolio review with a stronger product story, cleaner repository hygiene, release automation, and production-readiness documentation.

### What This Demonstrates

- ASP.NET Core 8 MVC application architecture
- EF Core + SQLite persistence
- CRUD workflows across multiple domain entities
- Service-layer recommendation and report logic
- Session authentication and anti-forgery validation
- Security response headers
- Google Cloud Run deployment readiness
- Health-check endpoint for operational validation
- CI workflow for restore, build, and publish validation
- Polished command-center UI suitable for screenshots and recruiter review

### Resume-Ready Summary

**Raiders Vault** is a production-style game companion platform built with ASP.NET Core MVC, EF Core, SQLite, Razor, and Google Cloud Run. It centralizes ARC Raiders planning through loadout management, quest tracking, blueprint collection, map-condition intelligence, skill planning, favorites, farm routes, and reporting.

See `docs/PRODUCTION_READINESS.md` and `docs/DEPLOYMENT_RUNBOOK.md` for portfolio talking points, validation steps, and deployment instructions.

## Latest Professional Upgrade

Raiders Vault now includes a MetaForge-inspired companion hub experience:

- Premium Blueprint Command Vault with card-based tracking, farm-plan previews, and collection metrics.
- Run Planner intelligence layer with priority score, risk level, route sequence, extraction guidance, threat controls, and operator tips.
- Professional companion-app workflow that keeps map, condition, item target, route, and checklist decisions together.
- No external API dependency is required; the system uses internal seeded data and local recommendation logic.


## v44 Interactive Companion Upgrade

This package adds a more professional companion-app experience inspired by modern loot trackers and MetaForge-style planning tools:

- Interactive tactical map board on Run Planner with clickable Entry, Loot Core, Blueprint, Objective, and Extraction zones.
- Visual skill tree network with branch cards, node states, recommended-path highlighting, and a click-to-inspect node panel.
- Reusable item icon system for weapons, shields, medical items, tools, gear, intel, and skill branches.
- Upgraded loadout and blueprint cards so records feel more like a polished game companion dashboard.
- Browser-side interactivity is handled in `wwwroot/js/site.js`; no database migration is required.
