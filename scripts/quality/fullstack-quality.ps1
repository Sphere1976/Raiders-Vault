param(
    [switch]$SkipNode,
    [switch]$SkipJava,
    [switch]$SkipInfra
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
Set-Location $root

Write-Host "== Raiders Vault quality gate ==" -ForegroundColor Cyan

Write-Host "Running ASP.NET Core build..." -ForegroundColor Yellow
dotnet build --no-restore /p:UseAppHost=false

Write-Host "Running ASP.NET Core tests..." -ForegroundColor Yellow
dotnet test tests/RaidersVault.Tests/RaidersVault.Tests.csproj --no-restore --collect:"XPlat Code Coverage"

if (-not $SkipNode) {
    Write-Host "Running Next.js checks..." -ForegroundColor Yellow
    Push-Location frontend/raiders-vault-next
    npm install
    npm run build
    npm run test:e2e
    Pop-Location

    Write-Host "Running mobile TypeScript check..." -ForegroundColor Yellow
    Push-Location mobile/raiders-vault-mobile
    npm install
    npm run typecheck
    Pop-Location
}

if (-not $SkipJava) {
    Write-Host "Running Spring service tests..." -ForegroundColor Yellow
    Push-Location Services/liveops-spring
    mvn test
    Pop-Location

    Write-Host "Running GraphQL BFF tests..." -ForegroundColor Yellow
    Push-Location gateway/graphql-bff
    mvn test
    Pop-Location
}

if (-not $SkipInfra) {
    Write-Host "Validating Terraform..." -ForegroundColor Yellow
    Push-Location infra/aws/terraform
    terraform fmt -check
    terraform init -backend=false
    terraform validate
    Pop-Location

    Write-Host "Rendering Kubernetes and linting Helm..." -ForegroundColor Yellow
    kubectl kustomize infra/kubernetes/base | Out-Null
    helm lint infra/helm/raiders-vault
}

Write-Host "Quality gate completed." -ForegroundColor Green
