# media-portfolio-backend — Task Plan

> Source of truth: `API-Contract-v1.0.docx` and
> `Requirements-and-Architecture.docx`, both at this repo's root. If a
> task here ever conflicts with those documents, the documents win.

## Team note
2 people: 1 backend developer (this repo), 1 frontend developer
(`media-portfolio-frontend`). Backend endpoints for a given phase should
land and be merged before the frontend dev starts building against
them — building a UI against a guessed response shape creates rework
once the real one lands. Target: **3 weeks** internally, against a
1-month client deadline — the buffer is intentional, not slack to fill
with extra scope.

---

## Phase 0 — Foundation
See `.agents/master-prompt-phase0-foundation.md` for the full task
breakdown and exit criteria.

**Exit criteria:** `docker compose up` works, seeded admin can log in
and get a JWT, `/health` responds, CI is green.

## Phase 1 — Categories & Secondary Services
- [ ] `Category` CRUD (admin-only): create, edit (NameEn/NameAr), delete
      (per `cqrs-mediatr-patterns.md`'s non-cascade deletion rule),
      reorder if needed
- [ ] `GET /categories` (public, list all)
- [ ] `SecondaryService` CRUD (admin-only): create, edit, delete,
      reorder (`DisplayOrder`)
- [ ] `GET /secondary-services` (public, list all)

**Exit criteria:** admin can fully manage categories and services
end-to-end; public endpoints return them correctly ordered and bilingual.

## Phase 2 — Media Upload Pipeline & Video CRUD
- [ ] Cloudflare Stream integration: request direct-upload URL, receive
      upload-complete confirmation, persist `CloudflareStreamId`
- [ ] Cloudflare Images integration: same pattern for manually-uploaded
      thumbnails; auto-generated-thumbnail fallback path when admin
      skips manual upload
- [ ] `Video` CRUD (admin-only): create (title, description, category
      assignments, upload), edit, delete (soft), publish/unpublish
- [ ] `GET /admin/videos` (admin-only, includes drafts + admin-only
      fields: title, description, all analytics counters)
- [ ] `POST /videos/{id}/categories` (assign/unassign, many-to-many)

**Exit criteria:** admin can upload a video end-to-end (with or without
a manual thumbnail), assign it to multiple categories, and toggle
draft/published status; Cloudflare asset IDs are correctly persisted.

## Phase 3 — Public Browse, Search & Tracking
- [ ] `GET /videos?categoryId=` (public, published only, no title/
      description in response)
- [ ] `GET /videos/search?q=` (public, matches against `TitleAr` +
      `DescriptionAr` server-side, published only, response still omits
      title/description per the "hidden from public" rule)
- [ ] `GET /videos/{id}` (public, published only, single video detail
      for the play page)
- [ ] `POST /videos/{id}/track-view` (public, increments `ViewCount`)
- [ ] `POST /videos/{id}/track-whatsapp-click` (public, increments
      `WhatsAppClickCount`)
- [ ] `GET /site-settings` (public: logo, social links, WhatsApp number
      + resolved template for the requested `Accept-Language`)

**Exit criteria:** a visitor can browse by category, search, view a
video, and both tracking endpoints correctly increment their counters
without exposing the admin-only title/description fields anywhere in
the public response shapes.

## Phase 4 — Contact Form & Retention
- [ ] `POST /contact-submissions` (public: name, phone, email, message,
      optional `originCategoryId`/`originVideoId` — increments the
      origin video's `FormSubmissionCount` if present)
- [ ] `GET /admin/contact-submissions` (admin-only, list + mark-read)
- [ ] `DELETE /admin/contact-submissions/{id}` (admin-only, manual
      delete, any age/status)
- [ ] Hangfire daily job: hard-delete `Read` submissions older than 30
      days
- [ ] Rate limiting on `POST /contact-submissions` and
      `POST /auth/login`

**Exit criteria:** a full submit → admin views → marks read → auto-
deletes-after-30-days cycle is verified (test the job logic directly,
not by waiting 30 real days).

## Phase 5 — Site Settings, Analytics Dashboard & Hardening
- [ ] `PUT /admin/site-settings` (logo, social links, WhatsApp number +
      both language templates — admin-editable)
- [ ] `GET /admin/analytics/videos` (per-video view/click/submission
      counts for the dashboard)
- [ ] Security review: auth edge cases, ownership checks (not very
      applicable with a single admin, but verify no endpoint is
      accidentally anonymous that shouldn't be)
- [ ] Confirm CORS, error-code registry, and contract version are fully
      in sync with what `media-portfolio-frontend` actually consumes
- [ ] Deploy to Azure App Service staging slot, smoke test against it

**Exit criteria:** feature-complete per the Requirements doc; staging
deployment verified; ready for the client's domain cutover.
