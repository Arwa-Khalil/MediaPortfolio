# Security & Auth — media-portfolio-backend

## Single admin account — still a real auth system
There is exactly one admin user for this system (per the Requirements
doc). This does **not** mean auth can be hard-coded, skipped, or
simplified into a shared secret. Build it as a normal, secure
authentication flow:
- Password hashed with ASP.NET Core Identity's default hasher (or
  BCrypt) — never plaintext, never a reversible encryption.
- JWT access token (short-lived, ~1h) + refresh token flow, matching the
  API Contract's Auth section exactly.
- `POST /auth/login`, `POST /auth/refresh`, `POST /auth/logout`,
  `POST /auth/forgot-password`, `POST /auth/reset-password`,
  `POST /auth/change-password` — all five required per the confirmed
  scope, not optional extras.
- Forgot-password flow sends a time-limited reset token via email (see
  Infrastructure's email sender) — never emails the actual password.

## Authorization
Every dashboard-only endpoint (anything under categories, videos,
services, settings, contact-submissions management, analytics) requires
a valid admin JWT. Public endpoints (category listing, video search/
browse, contact-form submission, WhatsApp-click/view tracking) are
anonymous by design — do not accidentally lock these behind auth.

## Rate limiting
Apply rate limiting to: `POST /auth/login` (prevent brute force),
`POST /contact-submissions` (prevent spam flooding even with Turnstile
in front of it on the frontend — defense in depth), and the
analytics-tracking endpoints (view/click increments) to prevent trivial
counter-inflation abuse.

## Input validation
Every write endpoint validated via FluentValidation before it reaches
the handler (enforced by the `ValidationBehavior` pipeline behavior — see
`cqrs-mediatr-patterns.md`). This includes the contact form (email format,
phone format, message length caps) and every admin CRUD input.

## CORS
Configured explicitly for the `media-portfolio-frontend` deployed origin
(and `localhost` dev origin) — never a wildcard `*` in production, since
authenticated admin requests carry a bearer token.

## Secrets
Cloudflare Stream/Images API tokens, DB connection string, JWT signing
key, and email provider credentials are read from environment
variables / configuration, never hard-coded or committed. See
`docker-and-config.md`.
