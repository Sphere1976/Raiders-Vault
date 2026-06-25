# Full-Stack Job Alignment

This document maps Raiders Vault to a React, TypeScript, Next.js, Java, Spring Boot, AWS,
Terraform, testing, and CI/CD full-stack engineering role.

## Requirement Matrix

| Job requirement | Raiders Vault evidence |
|---|---|
| React, TypeScript, JavaScript, Next.js | `frontend/raiders-vault-next` contains a typed Next.js application, reusable React components, server-rendered data loading, and strict TypeScript settings. |
| Mobile/cross-platform product thinking | `mobile/raiders-vault-mobile` contains an Expo/React Native scaffold that reuses the LiveOps domain model for a pocket operations console. |
| Java and Spring Boot backend | `Services/liveops-spring` contains a Spring Boot REST service with controller, service, records, actuator, and JUnit/MockMvc test coverage. |
| GraphQL integration pattern | `gateway/graphql-bff` contains a Spring GraphQL backend-for-frontend facade for executive Global Ops queries. |
| RESTful APIs and web services | Existing ASP.NET endpoint `/api/v1/global-ops` plus Spring endpoint `/api/v1/live-ops`; Postman collection documents both. |
| AWS cloud architecture | `infra/aws/terraform` and `infra/aws/cloudformation` define VPC, subnets, ALB, ECS Fargate, IAM execution role, CloudWatch logs, and security groups. |
| Infrastructure as Code | Terraform files include provider versions, variables, outputs, and validation instructions; CloudFormation provides an alternate AWS-native template. |
| Kubernetes platform readiness | `infra/kubernetes/base` and `infra/helm/raiders-vault` model deployments, services, ingress, probes, resource limits, Kustomize, and Helm packaging. |
| Playwright tests | `frontend/raiders-vault-next/tests/global-ops.spec.ts` verifies the Next.js global operations dashboard. |
| ASP.NET Core tests | `tests/RaidersVault.Tests` uses xUnit and `WebApplicationFactory` for health, security header, authentication, and protected API checks. |
| JUnit tests | `Services/liveops-spring/src/test/.../LiveOpsControllerTest.java` validates the Spring Boot REST contract. |
| Cucumber tests | `Services/liveops-spring/src/test/resources/features/live_ops.feature` documents executable BDD acceptance criteria for live-ops API behavior. |
| Postman tests | `tests/postman/RaidersVault.postman_collection.json` includes health, protected API, and Spring service checks. |
| CI/CD and DevOps | `.github/workflows/fullstack-ci.yml` builds ASP.NET Core, builds Next.js, runs Playwright, runs Maven tests, and validates Terraform. |
| Release governance | `.github/PULL_REQUEST_TEMPLATE.md`, issue templates, CODEOWNERS, Dependabot, and `docs/release/RELEASE_CHECKLIST.md` model team workflow and dependency hygiene. |
| Security automation | `.github/workflows/security-sbom.yml` adds CodeQL, dependency review, and SPDX SBOM generation. |
| API contracts | `docs/api/raiders-vault-openapi.yaml` describes MVC and Spring REST endpoints with shared schema definitions. |
| Observability and SLOs | `docs/observability/SLO_AND_TELEMETRY.md` defines golden signals, SLOs, alert examples, and telemetry architecture. |
| Security analysis | `docs/security/THREAT_MODEL.md` maps assets, trust boundaries, STRIDE risks, mitigations, and security backlog. |
| Performance engineering | `tests/performance/global-ops.k6.js` defines k6 thresholds for health and Item Database response behavior. |
| Resilience engineering | `tests/chaos` documents failure drills and includes a degraded Global Ops k6 scenario. |
| Policy as code | `infra/policy` contains Rego policies for Kubernetes deployment guardrails. |
| Event-driven architecture | `infra/eventbridge` and `docs/diagrams/event-driven-architecture.mmd` model EventBridge-based platform events. |
| Analytics engineering | `infra/data-warehouse` and `docs/data/ANALYTICS_STRATEGY.md` define warehouse facts, views, governance, and data products. |
| Performance, quality, responsiveness | Item Database paging and inline updates reduce full-page redraws; Next.js frontend uses server-rendered data and small typed components. |
| Code quality and organization | Monorepo is separated by bounded responsibility: `frontend`, `services`, `infra`, `tests`, existing MVC app, and docs. |
| Product and architecture collaboration | ADRs in `docs/adr` and product requirements in `docs/product/GLOBAL_OPS_PRD.md` explain tradeoffs, interfaces, deployment boundaries, and modernization path. |
| Architecture communication | `docs/diagrams/fullstack-platform.mmd` provides a Mermaid system diagram for reviewers and architecture discussions. |
| Application storytelling | `docs/portfolio`, `docs/interview`, and `docs/compliance` contain a case study, recruiter one-pager, STAR stories, and engineering scorecard. |

## Suggested Resume Bullets

- Expanded Raiders Vault into a full-stack monorepo with ASP.NET Core MVC, Next.js, TypeScript, Java Spring Boot, REST APIs, Terraform AWS infrastructure, and CI/CD validation.
- Built a typed Next.js operations console that consumes Raiders Vault live-ops data and demonstrates React component design, SSR data loading, and Playwright coverage.
- Implemented a Spring Boot live-ops API with typed Java records, controller/service separation, actuator readiness, and JUnit/MockMvc contract tests.
- Added a Spring GraphQL backend-for-frontend and Expo/React Native mobile scaffold to demonstrate multi-client API strategy.
- Authored Terraform, CloudFormation, Kubernetes, and Helm infrastructure for AWS ECS Fargate, ALB, VPC networking, IAM execution roles, CloudWatch logging, ingress, probes, and resource-governed service deployments.
- Added EventBridge, data warehouse SQL, Rego policy-as-code, OpenAPI, Postman, xUnit, Playwright, JUnit, Cucumber, k6, Docker Compose, CodeQL, SBOM generation, Dependabot, PR templates, CODEOWNERS, and GitHub Actions quality gates for API validation, frontend E2E testing, backend builds, Java tests, infrastructure validation, and release governance.

## Interview Talking Points

- The original capstone remains a working ASP.NET Core MVC product, while the new folders show how the platform would be decomposed into modern frontend, service, and cloud boundaries.
- The Next.js app intentionally treats the existing MVC API as a protected upstream service and falls back gracefully for local frontend-only demos.
- The Spring Boot service is a focused integration slice that can later ingest official data, publish cached live-ops snapshots, and serve both web and mobile clients.
- Terraform keeps the AWS deployment understandable for interview review while still modeling real production concerns such as load balancing, logs, health checks, and security groups.
- The CI workflow is organized by technology boundary, which makes failures easier for teams to triage.
