# Product Requirements: Global Ops Modernization

## Problem

Operators need a fast way to understand current ARC Raiders conditions, recent Embark updates, item
readiness, blueprint priorities, and regional operations signals without jumping between separate tools.

## Goals

- Provide a responsive global operations dashboard for desktop and mobile users.
- Expose a typed API contract for web, mobile, and external dashboard clients.
- Support progressive modernization through React/Next.js without replacing the working MVC product.
- Demonstrate production engineering practices through tests, CI/CD, and cloud infrastructure.

## Non-Goals

- Replace the ASP.NET Core MVC application in one migration.
- Store user secrets or external provider credentials in the repo.
- Claim real-time official data when a local fallback snapshot is being used.

## Personas

- Product owner: wants clear feature readiness and an understandable roadmap.
- Architect: wants service boundaries, API contracts, deployment options, and risk visibility.
- Full-stack engineer: wants typed frontend code, robust backend services, tests, and automation.
- End user: wants fast item lookup, live condition awareness, and reliable planning workflows.

## Functional Requirements

- Render Global Ops in the existing MVC application.
- Render a Next.js companion Global Ops dashboard with typed components.
- Provide REST contracts for MVC global ops and Spring live ops.
- Provide Postman, Playwright, JUnit, and Cucumber coverage for representative flows.
- Provide AWS Terraform and CloudFormation deployment blueprints.

## Quality Attributes

- Performance: Item Database uses paging and inline updates to avoid full-screen redraws.
- Reliability: Live ops clients fall back gracefully when protected upstream APIs are unavailable.
- Security: Protected MVC APIs retain session enforcement.
- Operability: Health endpoints, CloudWatch logs, ALB routing, and CI workflows are documented.
- Maintainability: Frontend, service, infrastructure, and test assets are separated by folder.

## Acceptance Criteria

- ASP.NET Core build passes.
- Next.js frontend can render the Global Ops page with fallback or upstream data.
- Spring Boot service returns at least one map condition and one news update.
- JUnit and Cucumber tests validate the Spring live-ops contract.
- Terraform and CloudFormation templates describe equivalent AWS ECS deployment paths.
- README and job-alignment documentation map the repo to the target role requirements.
