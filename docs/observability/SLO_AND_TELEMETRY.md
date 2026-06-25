# SLO and Telemetry Plan

## Service Level Objectives

| Journey | SLO | Measurement |
|---|---|---|
| Health check | 99.9% successful responses over 30 days | `/health`, ALB target health, Kubernetes probes |
| Item Database page | p95 under 750 ms for authenticated users | k6, browser timings, server request metrics |
| Global Ops API | p95 under 500 ms for cached/fallback data | API latency histogram |
| LiveOps service | 99.5% successful `/api/v1/live-ops` responses | Spring actuator and gateway metrics |

## Golden Signals

- Latency: MVC action duration, Next.js render duration, Spring request duration
- Traffic: requests per route and per service
- Errors: 4xx/5xx rates, failed upstream fetches, validation failures
- Saturation: CPU, memory, thread pool, database file locks, pod/task restarts

## Proposed Telemetry Stack

- OpenTelemetry instrumentation for ASP.NET Core and Spring Boot
- Prometheus-compatible metrics from Kubernetes or ECS sidecars
- CloudWatch logs for AWS Fargate deployments
- Structured JSON logs with correlation IDs
- Grafana dashboards for global operations, item database, and live-ops ingestion

## Alert Examples

- Health endpoint failing for 3 consecutive checks
- p95 latency above SLO for 10 minutes
- 5xx error rate above 2% for 5 minutes
- LiveOps fallback mode active longer than 30 minutes
- Container restart loop detected
