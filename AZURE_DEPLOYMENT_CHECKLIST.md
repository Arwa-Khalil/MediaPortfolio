# Azure App Service Deployment Checklist

This document serves as the guide for the final deployment to Azure App Service (Staging and Production slots).

## 1. Infrastructure Provisioning
- [ ] **Azure Database for PostgreSQL**: Provision a Burstable tier instance. Record the connection string.
- [ ] **Azure App Service**: Create a new Web App (Linux or Windows, .NET 10 runtime).
- [ ] **Staging Slot**: Create a deployment slot named `staging` attached to the main App Service.

## 2. Configuration Settings (Environment Variables)
In the Azure Portal, navigate to the App Service -> Configuration -> Application settings.
Configure the following as "Slot settings" where appropriate so staging and production can differ:

| Key | Description |
|---|---|
| `ConnectionStrings__Default` | PostgreSQL connection string. |
| `Jwt__SigningKey` | Secure >32 byte string for JWT. |
| `Jwt__AccessTokenLifetimeMinutes` | e.g. `60` |
| `Jwt__RefreshTokenLifetimeDays` | e.g. `30` |
| `MediaStorage__Provider` | `Cloudflare` |
| `Cloudflare__StreamApiToken` | From Cloudflare dashboard. |
| `Cloudflare__StreamAccountId` | From Cloudflare dashboard. |
| `Cloudflare__ImagesApiToken` | From Cloudflare dashboard. |
| `Cloudflare__ImagesAccountId` | From Cloudflare dashboard. |
| `Cloudflare__ImagesAccountHash` | The image delivery hash. |
| `Turnstile__SecretKey` | Cloudflare Turnstile secret key. |
| `Email__ApiKey` | SendGrid (or other) API Key. |
| `Email__FromAddress` | e.g. `no-reply@clientdomain.com` |
| `Cors__AllowedOrigin` | The deployed frontend URL (e.g. `https://staging.clientdomain.com`). |
| `Hangfire__Enabled` | `true` |
| `AdminSeed__InitialPassword` | The initial admin password. **Must be changed after first login.** |

## 3. Database Migration & Seeding
The backend application automatically applies Entity Framework Core migrations on startup (`db.Database.Migrate()` in `Program.cs`).
- [ ] The first startup will create all tables.
- [ ] The seed script will insert the initial `AdminUser` and the `SiteSettings` row.

## 4. Staging Verification Smoke Tests
After pushing code to the Staging slot, verify the following:
- [ ] **Health Check**: `GET /health` returns `200 OK`.
- [ ] **Auth**: Login to the dashboard using the seeded admin credentials.
- [ ] **Media**: Upload a video and verify it appears in Cloudflare Stream.
- [ ] **Background Jobs**: Verify the daily Hangfire retention job is registered.

## 5. Production Cutover
- [ ] Swap the `staging` slot with `production`.
- [ ] Perform a final verification against the production domain.
