# Interview STAR Stories

## Scaling a Capstone into a Platform

**Situation:** Raiders Vault began as a single ASP.NET Core MVC app.

**Task:** Make it credible for a full-stack role requiring React, TypeScript, Next.js, Java, Spring Boot,
AWS, Terraform, testing, and CI/CD.

**Action:** Expanded it into a monorepo with a Next.js frontend, Spring Boot service, GraphQL BFF, mobile
scaffold, AWS IaC, Kubernetes manifests, OpenAPI, Postman, Playwright, JUnit, Cucumber, k6, and CI workflows.

**Result:** The repo now maps directly to the job description and shows a practical modernization path.

## Fixing Performance and Responsiveness

**Situation:** The Item Database became heavy after importing hundreds of ARC Raiders items.

**Task:** Reduce visual flashing and improve perceived responsiveness.

**Action:** Added server-side paging and inline count updates with `fetch`, avoiding full-page redraws.

**Result:** The page renders a manageable set of cards and updates item counts without reloading the screen.

## Designing for Reliability

**Situation:** Live operations data can be unavailable or change frequently.

**Task:** Keep dashboards useful even when upstream data is unavailable.

**Action:** Added fallback live-ops snapshots, documented resilience drills, and defined observability/SLO
expectations for future monitoring.

**Result:** The platform has a clear reliability story around graceful degradation and operational visibility.
