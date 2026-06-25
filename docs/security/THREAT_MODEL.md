# Raiders Vault Threat Model

## Scope

This threat model covers the ASP.NET Core MVC app, Next.js companion console, Spring Boot LiveOps
service, REST APIs, deployment automation, and AWS/Kubernetes infrastructure blueprints.

## Assets

- User sessions and authentication state
- Inventory, blueprint, loadout, profile, and audit records
- Live operations data and external-source metadata
- CI/CD credentials and container images
- Cloud infrastructure state and logs

## Trust Boundaries

- Browser to MVC application
- Browser to Next.js application
- Next.js server to MVC REST API
- Next.js server to Spring Boot LiveOps service
- CI/CD runner to cloud provider APIs
- Public ingress/load balancer to internal service pods or ECS tasks

## STRIDE Review

| Threat | Example | Mitigation |
|---|---|---|
| Spoofing | Forged session or service identity | HttpOnly cookies, fixed-time password comparison, future OIDC, service-to-service auth roadmap |
| Tampering | Modified item counts or form posts | Anti-forgery validation, server-side validation, EF constraints, audit events |
| Repudiation | User denies API export or login | Audit events for login, logout, CSV export, and API access |
| Information disclosure | Sensitive config exposed in repo | `.gitignore`, no secrets in IaC, environment-driven config |
| Denial of service | Expensive database pages or repeated API calls | Item paging, health checks, cache headers, k6 thresholds, autoscaling roadmap |
| Elevation of privilege | Unauthorized admin access | Session enforcement, future RBAC and OIDC integration |

## Security Backlog

- Replace demo login with ASP.NET Core Identity or OIDC.
- Add role-based authorization policies.
- Add container image scanning in CI.
- Add secret scanning and branch protection.
- Add WAF rules for production ingress.
- Add service-to-service authentication for Next.js to backend calls.
