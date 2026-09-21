# Docker & Config — media-portfolio-backend

## Local dev: Docker Compose
```
docker-compose.yml
  api          # this repo, hot-reload via dotnet watch in dev
  postgres     # local Postgres, matches production major version
```
No Redis, no MinIO, no message queue — this project has no caching layer
or self-hosted file storage requirement (media lives in Cloudflare
Stream/Images, not locally). Do not add infrastructure containers
"for consistency with other projects" — keep this repo's Compose file to
what it actually uses.

## Environments
Three, matching the Requirements doc's plan: **local** (Docker Compose),
**staging** (Azure App Service, staging slot or a separate low-tier app),
**production** (Azure App Service, production slot). Configuration
differs only by environment variables / App Service configuration —
never by a code branch checking environment name.

## Configuration values (all via environment variables / Azure App
Service configuration, never committed)
- `ConnectionStrings__Default` — Postgres connection string.
- `Jwt__SigningKey`, `Jwt__AccessTokenLifetimeMinutes`,
  `Jwt__RefreshTokenLifetimeDays`.
- `Cloudflare__StreamApiToken`, `Cloudflare__StreamAccountId`,
  `Cloudflare__ImagesApiToken`, `Cloudflare__ImagesAccountId`.
- `Turnstile__SecretKey` — Cloudflare Turnstile secret key for contact form.
- `Email__ApiKey`, `Email__FromAddress` (for password reset emails).
- `Cors__AllowedOrigin` — the deployed frontend URL.
- `Hangfire__Enabled` — toggles the background job server (defaults to `true`).
- `AdminSeed__InitialPassword` — used once by the seed script, then the
  admin must change it (see `security-and-auth.md`).

## `appsettings.json` vs environment variables
`appsettings.json` holds only non-secret defaults and structure (log
levels, etc). Every secret and every per-environment value comes from
environment variables (Docker Compose `.env` locally, Azure App Service
Application Settings in staging/production) — this is what lets the same
Docker image be promoted from staging to production unchanged.

## Health check
`GET /health` — checks DB connectivity, returns `200` if reachable. Used
by Azure App Service's health-check feature and by CI post-deploy smoke
tests.
