<#
.SYNOPSIS
Production-style deployment script for Raiders Vault.

.DESCRIPTION
Cleans, restores, builds, publishes, and deploys Raiders Vault to Google Cloud Run.
Run this script from the repository root after authenticating with gcloud.
#>

param(
    [string]$ServiceName = "raiders-vault",
    [string]$Region = "us-central1",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

Write-Host "Raiders Vault Production Deployment" -ForegroundColor Green
Write-Host "Service: $ServiceName | Region: $Region | Configuration: $Configuration" -ForegroundColor DarkGray

Write-Step "Cleaning previous build artifacts"
dotnet clean

Write-Step "Restoring NuGet dependencies"
dotnet restore

Write-Step "Building the application in $Configuration mode"
dotnet build -c $Configuration --no-restore

Write-Step "Publishing deployable output"
dotnet publish -c $Configuration -o publish --no-build

Write-Step "Deploying to Google Cloud Run"
gcloud run deploy $ServiceName `
    --source . `
    --region $Region `
    --platform managed `
    --allow-unauthenticated

Write-Host "`nDeployment completed successfully." -ForegroundColor Green
Write-Host "Validate the application health endpoint at: /health" -ForegroundColor Yellow
