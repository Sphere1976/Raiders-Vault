# Raiders Vault Case Study

## Executive Summary

Raiders Vault is a production-style full-stack platform that started as an ASP.NET Core MVC capstone
and evolved into a multi-client, multi-service, cloud-ready engineering portfolio. It demonstrates
product delivery, frontend modernization, backend service design, REST and GraphQL APIs, infrastructure
as code, automated testing, security posture, observability planning, and release governance.

## Problem

Players need to plan ARC Raiders objectives, track inventory gaps, understand live map conditions, and
prioritize routes without manually combining notes, spreadsheets, and external websites.

For a hiring portfolio, the project also needed to demonstrate the engineering profile required by a
modern full-stack role:

- React, TypeScript, JavaScript, and Next.js
- Java and Spring Boot
- AWS architecture and Infrastructure as Code
- Tests across UI, API, backend, BDD, and performance layers
- CI/CD, security, quality, and collaboration workflows

## Solution

The project uses a staged modernization strategy:

- Keep the working ASP.NET Core MVC product as the core application.
- Add a typed Next.js companion console.
- Add Spring Boot REST and Spring GraphQL services.
- Add Expo/React Native mobile scaffold.
- Add AWS Terraform and CloudFormation infrastructure.
- Add Kubernetes, Helm, Docker Compose, EventBridge, warehouse SQL, and policy-as-code artifacts.
- Add Playwright, xUnit, JUnit, Cucumber, Postman, and k6 quality gates.

## Engineering Highlights

- Built protected API workflows and audit event capture.
- Imported and paged a large item dataset while preserving UI responsiveness.
- Added live operations cards and fallback behavior for upstream instability.
- Modeled deployment across containers, ECS Fargate, Kubernetes, Helm, and local Docker Compose.
- Created product, architecture, security, observability, release, and roadmap documentation.

## Outcome

Raiders Vault now presents as a full-stack platform program rather than a narrow CRUD app. It provides
evidence for frontend engineering, backend service design, cloud architecture, test automation, DevOps,
product collaboration, and senior-level systems thinking.
