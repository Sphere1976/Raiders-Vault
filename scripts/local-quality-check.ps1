$ErrorActionPreference = "Stop"

Write-Host "Running Raiders Vault local quality gate..." -ForegroundColor Cyan

dotnet clean
dotnet restore
dotnet build -c Release --no-restore

dotnet publish -c Release -o publish --no-build

Write-Host "Quality gate completed: restore, build, and publish succeeded." -ForegroundColor Green
