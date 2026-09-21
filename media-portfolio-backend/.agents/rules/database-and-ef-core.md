# Database & EF Core — media-portfolio-backend

## Provider
PostgreSQL via Npgsql, targeting Azure Database for PostgreSQL in
staging/production, a local Postgres container in dev (see
`docker-and-config.md`).

## Core tables (see API Contract for full field-level detail — this is
the summary; the contract owns the authoritative shape)
- `Categories` — Id, NameEn, NameAr, CreatedAt.
- `Videos` — Id, TitleAr (admin-only), DescriptionAr (admin-only, search
  field), CloudflareStreamId, CloudflareThumbnailImageId (nullable —
  null means auto-generated thumbnail was used), Status
  (Draft/Published), ViewCount, WhatsAppClickCount,
  FormSubmissionCount, CreatedAt, PublishedAt (nullable).
- `VideoCategories` — join table, VideoId, CategoryId, composite key.
- `SecondaryServices` — Id, TitleEn, TitleAr, DescriptionEn,
  DescriptionAr, DisplayOrder, CreatedAt.
- `ContactSubmissions` — Id, Name, Phone, Email, Message,
  OriginCategoryId (nullable), OriginVideoId (nullable), Status
  (Unread/Read), CreatedAt, ReadAt (nullable).
- `SiteSettings` — single-row table: LogoImageId, SnapchatUrl,
  TiktokUrl, InstagramUrl, WhatsAppNumber, WhatsAppTemplateAr,
  WhatsAppTemplateEn.
- `AdminUsers` — Id, Email, PasswordHash, PasswordResetToken (nullable),
  PasswordResetTokenExpiresAt (nullable).

## Soft deletes
Videos and Categories use soft delete (an `IsDeleted` flag + a global EF
Core query filter), not hard delete — this preserves analytics history
(view/click counts) even if content is later removed. Contact
submissions are the one exception: they are hard-deleted per the 30-day
retention job and on manual admin delete, by design (see Requirements
doc's Contact Form section) — do not add soft-delete to that table.

## Migrations
Every schema change is a migration, generated via
`dotnet ef migrations add <Phase>_<Description>` — never a hand-edited
migration file, never a direct schema change against a running database
outside of a migration.

## Seeding
A seed script creates: the single `AdminUsers` row (from an env-var-
provided initial password, forced-change-on-first-login), the initial
category list (the seven categories in the Requirements doc, Arabic
names as given by the client, English names to be provided by
translation before seeding), and a default `SiteSettings` row with the
two WhatsApp template drafts from the Requirements doc as the starting
values (admin-editable from day one).

## Query performance
Category-browse and search queries are the highest-traffic public reads.
Index `Videos.Status`, and the `VideoCategories` join table on both
columns. No caching layer (e.g. Redis) is planned for v1 — traffic
volume at this scale doesn't warrant the added infrastructure; revisit
only if real usage shows otherwise.
