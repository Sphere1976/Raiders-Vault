# ADR 0001: Full-Stack Monorepo Modernization

## Status

Accepted

## Context

Raiders Vault began as an ASP.NET Core MVC capstone application. The target full-stack role expects
evidence across React, TypeScript, Next.js, Java, Spring Boot, AWS, Terraform or CloudFormation,
testing frameworks, CI/CD, RESTful APIs, and collaborative architecture thinking.

Replacing the working MVC app would risk destabilizing the product. Keeping only documentation would
not demonstrate implementation capability.

## Decision

Use a monorepo modernization path:

- Keep the existing ASP.NET Core MVC application as the core product.
- Add `frontend/raiders-vault-next` as a React, TypeScript, and Next.js companion console.
- Add `Services/liveops-spring` as a Java Spring Boot integration service.
- Add `infra/aws/terraform` and `infra/aws/cloudformation` as cloud deployment blueprints.
- Add `tests/postman`, Playwright, JUnit, Cucumber, and GitHub Actions as test and automation evidence.

## Consequences

- Reviewers can inspect concrete code for each required technology.
- The current product remains runnable and demonstrable.
- Future work can migrate features gradually behind REST contracts rather than forcing a rewrite.
- The repo is larger, so documentation must clearly explain bounded responsibilities.
