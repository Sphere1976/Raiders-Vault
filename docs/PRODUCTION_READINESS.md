# Raiders Vault Production Readiness

Raiders Vault is presented as a production-style ASP.NET Core MVC portfolio application, not a disposable class assignment. The project demonstrates full-stack planning workflows, data persistence, hosted deployment, security-minded middleware, and a polished companion-app user experience.

## Portfolio Positioning

**Application type:** Game companion planning platform for ARC Raiders  
**Primary stack:** ASP.NET Core 8 MVC, Entity Framework Core, SQLite, Razor views, CSS, Google Cloud Run  
**Key workflows:** Loadout CRUD, quest tracking, blueprint collection, farm planning, map condition intelligence, skill planning, favorites, reporting, profile-driven recommendations

## Production-Quality Improvements

- Centralized premium UI system in `wwwroot/css/site.css` for consistent dashboard, cards, tables, forms, buttons, and status badges.
- Command-center dashboard that summarizes saved kits, pinned intel, objective progress, blueprint progress, recommended run strategy, and core modules.
- Security middleware that adds common browser protection headers including content type protection, frame protection, referrer policy, permissions policy, and content security policy.
- Session configuration using HttpOnly, SameSite Strict, production Secure cookies, and a named application session cookie.
- Health check endpoint at `/health` for deployment verification and uptime checks.
- Cloud Run deployment script under `scripts/deploy-cloudrun.ps1` for repeatable release deployment.
- Local quality gate script under `scripts/local-quality-check.ps1` for restore, build, and publish validation.
- GitHub Actions CI workflow under `.github/workflows/dotnet-ci.yml` to validate restore, build, and publish on source changes.
- Documentation files for architecture, UI upgrades, deployment notes, security posture, and resume positioning.

## Resume Bullets

- Built **Raiders Vault**, a production-style ASP.NET Core 8 MVC planning platform with CRUD workflows for loadouts, quests, blueprints, favorites, reports, profile settings, and map-based recommendations.
- Designed a premium Razor/CSS command-center dashboard with progress metrics, tactical workflow cards, search/filter pages, and portfolio-ready UI polish.
- Implemented EF Core with SQLite persistence, seeded application data, service-layer recommendation logic, session-based authentication, and anti-forgery validation.
- Prepared the application for cloud hosting with Google Cloud Run deployment scripts, Docker support, health checks, security headers, and repeatable build/publish automation.

## Interview Talking Points

1. **Problem:** ARC Raiders planning data can be scattered across notes, websites, and manual checklists.
2. **Solution:** Raiders Vault centralizes loadouts, quests, blueprint collection, map conditions, skill planning, and reporting in one web application.
3. **Architecture:** MVC controllers handle workflows, EF Core manages persistence, services isolate recommendation/report logic, and Razor views render the interface.
4. **Production mindset:** The project includes security headers, anti-forgery validation, health checks, release scripts, CI configuration, and cloud deployment support.
5. **Business value:** The app converts scattered game planning into a structured decision-support tool with measurable progress tracking.

## Final Validation Checklist

Before using this on a resume or sharing the GitHub repository, run:

```powershell
./scripts/local-quality-check.ps1
```

Then verify:

- The application launches locally.
- Login works.
- Dashboard metrics load.
- Loadout, Quest, and Blueprint CRUD flows work.
- Blueprint farm plans open.
- Run Planner recommendations render.
- Reports generate correctly.
- `/health` returns healthy.
- Cloud Run deployment succeeds.
