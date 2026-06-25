# ADR 0002: LiveOps Service Boundary

## Status

Accepted

## Context

Live ARC Raiders map conditions and news feeds are operational data that may change more often than
inventory, profile, or planning data. This makes live ops a good candidate for a separate service
boundary with caching, integration tests, and independent scaling.

## Decision

Model live operations as a Spring Boot REST service while the existing ASP.NET Core application remains
the system of record for user-facing planning data.

The Spring service exposes:

```text
GET /api/v1/live-ops
```

The Next.js frontend can consume either the protected ASP.NET Core global-ops API or the Spring live-ops
service, depending on deployment needs.

## Consequences

- Java and Spring Boot experience is represented by meaningful service code.
- Live operations can later support scheduled ingestion, caching, retries, observability, and eventing.
- The boundary is narrow enough to test with JUnit, Cucumber, Postman, and Playwright without requiring
  a large distributed system.
