# Security Policy

## Current Security Controls

- Global anti-forgery validation
- HttpOnly session cookies
- SameSite Strict session policy
- Secure cookies in deployed environments
- Fixed-time password hash comparison
- Input sanitization helper
- Security headers middleware
- Content Security Policy
- Health check endpoint for platform monitoring

## Production Recommendations

Before using this application outside a portfolio/demo environment:

1. Replace seeded credentials with ASP.NET Core Identity or external OAuth.
2. Move secrets to environment variables or a secret manager.
3. Replace SQLite with a managed production database.
4. Enable HTTPS-only deployment.
5. Add audit logging for sensitive record changes.
6. Add rate limiting to login and API endpoints.
7. Add dependency vulnerability scanning to CI.
