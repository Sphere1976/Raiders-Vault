# Full-Stack Runbook

This runbook describes how to operate the expanded Raiders Vault portfolio stack.

## Local MVC App

```bash
dotnet restore
dotnet build
dotnet run --urls http://127.0.0.1:5217
```

Open:

```text
http://127.0.0.1:5217
```

Default development login:

```text
admin / password
```

## Next.js Frontend

```bash
cd frontend/raiders-vault-next
npm install
npm run dev
```

Open:

```text
http://127.0.0.1:3000/global-ops
```

## Spring Boot LiveOps Service

```bash
cd Services/liveops-spring
mvn spring-boot:run
```

Open:

```text
http://127.0.0.1:8080/api/v1/live-ops
```

## GraphQL BFF

```bash
cd gateway/graphql-bff
mvn spring-boot:run
```

Open:

```text
http://127.0.0.1:8090/graphiql
```

## Mobile Scaffold

```bash
cd mobile/raiders-vault-mobile
npm install
npm run start
```

## Local Container Stack

```bash
docker compose -f docker-compose.fullstack.yml up --build
```

Services:

- MVC app: `http://127.0.0.1:5217`
- Next.js console: `http://127.0.0.1:3000`
- Spring service: `http://127.0.0.1:8080/api/v1/live-ops`

## Test Commands

```bash
dotnet build --no-restore /p:UseAppHost=false
dotnet test tests/RaidersVault.Tests/RaidersVault.Tests.csproj --no-restore --collect:"XPlat Code Coverage"
cd frontend/raiders-vault-next && npm run test:e2e
cd Services/liveops-spring && mvn test
```

Postman collection:

```text
tests/postman/RaidersVault.postman_collection.json
```

## Infrastructure Validation

Terraform:

```bash
cd infra/aws/terraform
terraform fmt -check
terraform init -backend=false
terraform validate
```

CloudFormation:

```bash
aws cloudformation validate-template \
  --template-body file://infra/aws/cloudformation/raiders-vault-ecs.yml
```

Kubernetes and Helm:

```bash
kubectl kustomize infra/kubernetes/base
helm lint infra/helm/raiders-vault
```

Performance smoke test:

```bash
k6 run tests/performance/global-ops.k6.js
```

Policy-as-code:

```bash
kubectl kustomize infra/kubernetes/base > /tmp/raiders-vault.yaml
conftest test /tmp/raiders-vault.yaml --policy infra/policy
```

## CI/CD

GitHub Actions workflow:

```text
.github/workflows/fullstack-ci.yml
```

The workflow builds the ASP.NET Core app, builds the Next.js app, runs Playwright tests,
runs Maven/JUnit/Cucumber tests, validates Terraform, and statically renders Kubernetes/Helm manifests.

Security workflow:

```text
.github/workflows/security-sbom.yml
```

This workflow runs CodeQL, dependency review, and SBOM generation.
