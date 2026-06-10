# Raiders Vault Deployment Runbook

## Prerequisites

- .NET 8 SDK
- Google Cloud SDK
- Authenticated gcloud account
- Google Cloud project with billing enabled
- Cloud Run and Cloud Build APIs enabled

## Local Release Validation

```powershell
./scripts/local-quality-check.ps1
```

## Deploy to Cloud Run

```powershell
./scripts/deploy-cloudrun.ps1 -ServiceName raiders-vault -Region us-central1
```

## Post-Deployment Checks

1. Open the Cloud Run service URL.
2. Confirm the login page loads over HTTPS.
3. Confirm `/health` returns a healthy response.
4. Log in and review dashboard metrics.
5. Open Loadouts, Quests, Blueprints, Run Planner, Skills, Reports, and Map Conditions.
6. Create, edit, search, and delete test data where appropriate.

## Rollback Strategy

Cloud Run retains revisions. If a release fails, use the Cloud Run console to route traffic back to the last stable revision.

## Resume Note

This deployment approach demonstrates repeatable release automation, managed container hosting, HTTPS hosting, health-check validation, and cloud operations awareness.
