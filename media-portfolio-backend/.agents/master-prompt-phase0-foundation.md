# Master Prompt — media-portfolio-backend, Phase 0: Foundation

You are building the backend for a bilingual (Arabic/English) media
portfolio website's admin API. This is a **separate repository** from
the frontend (`media-portfolio-frontend`) — the two communicate only
through the HTTP API defined in `API-Contract-v1.0.docx`, kept at this
repo's root.

## Before writing any code
Read, in this order:
1. `Requirements-and-Architecture.docx` (also at this repo's root) —
   the full product spec: what the site does, who the one admin user is,
   the video upload/publish/search flow, the WhatsApp-click and
   contact-form tracking behavior, and the confirmed technology choices.
2. `API-Contract-v1.0.docx` — the exact routes, request/response shapes,
   error codes, and enums you are implementing against.
3. Every file in `.agents/rules/` — these are binding constraints, not
   suggestions. In particular: `architecture.md` (Clean Architecture,
   modular monolith, the media-upload boundary rule), `database-and-ef-
   core.md` (the schema you're building toward), `security-and-auth.md`
   (the five required auth endpoints), and `definition-of-done.md`
   (what "done" means for every task below).

## Phase 0 scope
Build the foundation everything else sits on top of. Do not build any
Category/Video/Service/Contact feature logic yet — that's Phase 1+.

- [ ] Solution skeleton exactly per `.agents/rules/architecture.md`'s
      four-project layout (`Domain`, `Application`, `Infrastructure`,
      `API`) plus the three test projects.
- [ ] Docker Compose: `api` + `postgres` only (see
      `.agents/rules/docker-and-config.md` for why no more than that).
- [ ] EF Core `DbContext` with the full schema from
      `.agents/rules/database-and-ef-core.md`, including the
      `AdminUsers` seed row and the seven initial categories (Arabic
      names from the Requirements doc; leave `NameEn` as a clearly
      marked placeholder pending translation — do not invent English
      category names).
- [ ] Initial migration, verified against a clean local Postgres.
- [ ] JWT auth: `POST /auth/login`, `/refresh`, `/logout`,
      `/forgot-password`, `/reset-password`, `/change-password` — all
      six, matching the contract's Auth section exactly.
- [ ] CORS configured for the frontend repo's dev origin.
- [ ] Universal response envelope + global exception middleware per
      `.agents/rules/error-handling.md`.
- [ ] `GET /health`.
- [ ] CI pipeline: build + test on every push.

## Exit criteria
A developer can `docker compose up`, the API responds on
`GET /health`, the seeded admin can log in and receive a valid JWT, and
a `dotnet test` run passes with zero failures. The
`media-portfolio-frontend` repo should be able to successfully call
`/auth/login` cross-origin against this running instance before Phase 0
is considered closed.

## What NOT to do in this phase
Do not start on Category, Video, SecondaryService, ContactSubmission, or
Analytics endpoints yet, even if they seem quick — Phase 0 is foundation
only, so the next phase starts from a verified-solid base rather than
layering feature work on top of an unreviewed skeleton.
