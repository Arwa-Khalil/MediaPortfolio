# Architecture Rules — media-portfolio-backend

## Repository scope
This repository contains the backend ONLY. The frontend lives in a separate
repository, `media-portfolio-frontend`. The two communicate exclusively
through the HTTP API defined in `API-Contract-v1.0.docx` (kept at this
repo's root) — there is no shared build, no shared commit history, and no
compile-time coupling between them. Any change to a route, DTO shape,
status code, or enum value here is a contract change the frontend repo
depends on; treat it with the same care as a public API.

## Clean Architecture, Modular Monolith

```
src/
  MediaPortfolio.Domain/         # zero framework references. Entities, Value Objects, Enums, invariants.
  MediaPortfolio.Application/    # MediatR Commands/Queries/Handlers, DTOs, FluentValidation validators, interfaces
                                  #   (IRepository, IVideoStorageService, IImageStorageService,
                                  #    IEmailSender, IAnalyticsCounter)
  MediaPortfolio.Infrastructure/ # EF Core + Npgsql, Cloudflare Stream client, Cloudflare Images client,
                                  #   email sender, Hangfire jobs
  MediaPortfolio.API/            # Controllers, DTOs at the wire boundary, middleware, Program.cs
tests/
  MediaPortfolio.Domain.UnitTests/
  MediaPortfolio.Application.UnitTests/
  MediaPortfolio.API.IntegrationTests/
```

**Dependency rule, non-negotiable:** dependencies point inward only.
`Domain` depends on nothing. `Application` depends on `Domain` only.
`Infrastructure` depends on `Application` (to implement its interfaces) and
`Domain`. `API` depends on `Application` (and wires up `Infrastructure` at
the composition root in `Program.cs` only).

## Modules (bounded contexts within the monolith)
`Identity` (single admin auth, password reset/change), `Catalog`
(Categories + Videos + many-to-many), `Services` (secondary services
list), `Contact` (form submissions + retention job), `Settings` (logo,
social links, WhatsApp templates), `Analytics` (per-video view / WhatsApp-
click / form-submission counters).

Each module is a folder under `MediaPortfolio.Application/Features/<Module>/`
with its own Commands, Queries, Handlers, and Validators. A handler in one
module must not directly query another module's tables — go through that
module's public interface instead. This project is small enough that this
rule is easy to skip "just this once" — don't. It's what keeps Analytics
(which touches Catalog) and Contact (which touches Catalog for the
origin-video reference) from turning into a tangle.

## Why Clean Architecture even for a project this size
There is real, non-trivial domain logic here even though it reads as
"mostly CRUD": the video draft/publish lifecycle, the many-to-many
category assignment, the contact-submission 30-day retention rule, and the
WhatsApp template placeholder resolution are all worth unit-testing
without a database. Don't relax the dependency rule because a given
endpoint "is just CRUD" — it applies uniformly, and it's what lets two
developers (one per repo) work without stepping on each other via the
contract.

## No real-time infrastructure
This project has no feature that needs a live/pushed connection (no
group-order-style shared state, no chat). Do not introduce SignalR,
WebSockets, or a pub/sub layer for anything — every read is a normal
request/response. If a future feature seems to need real-time updates,
that needs an explicit decision, not an incidental addition.

## Media handling — the critical boundary
Raw video and image **bytes never pass through this API**. The API only
ever handles:
1. Requesting a direct/signed upload URL from Cloudflare Stream (video) or
   Cloudflare Images (images) on the admin's behalf.
2. Receiving a webhook/callback (or a client-reported completion, per the
   API Contract §on Media Uploads) confirming the upload finished, and
   persisting the returned Cloudflare asset ID + metadata.
3. Returning stored Cloudflare asset IDs/URLs to the frontend for playback
   and display.

Do not add an endpoint that accepts a video or image file body directly
into this API. If a future requirement seems to need that, treat it as a
contract change requiring explicit sign-off, not something to implement
quietly.
