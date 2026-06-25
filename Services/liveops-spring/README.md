# Raiders Vault LiveOps Spring Service

This service demonstrates the Java and Spring Boot part of the Raiders Vault modernization track.
It exposes a typed REST endpoint that can be consumed by the Next.js frontend, Postman collection,
or a future event-processing layer.

## Run

```bash
mvn spring-boot:run
```

## Test

```bash
mvn test
```

The test suite includes:

- JUnit and MockMvc controller coverage.
- Cucumber BDD acceptance coverage in `src/test/resources/features/live_ops.feature`.

## Endpoint

```text
GET /api/v1/live-ops
```

The current implementation uses deterministic sample data so the service remains easy to run in a
portfolio or interview environment. A production implementation would swap `LiveOpsService` for
scheduled upstream ingestion, caching, metrics, and persistence.

## Container

```bash
docker build -t raiders-vault-liveops .
docker run -p 8080:8080 raiders-vault-liveops
```
