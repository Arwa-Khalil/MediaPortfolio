# Naming Conventions — media-portfolio-backend

## C# / .NET
- PascalCase: classes, methods, properties, public fields, enum members.
- camelCase: local variables, method parameters, private fields prefixed
  with `_` (e.g. `_videoRepository`).
- Interfaces prefixed `I` (`IVideoStorageService`).
- Async methods suffixed `Async` (`GetVideoByIdAsync`).
- Commands named `<Verb><Noun>Command` (`CreateVideoCommand`,
  `PublishVideoCommand`). Queries named `<Verb><Noun>Query`
  (`GetVideosByCategoryQuery`, `SearchVideosQuery`).
- Handlers named exactly `<CommandOrQueryName>Handler`.
- DTOs at the Application layer suffixed `Dto`
  (`VideoDto`, `CategoryDto`). DTOs at the API/wire boundary suffixed
  `Request` / `Response` matching the API Contract's exact field names —
  see `api-contract-mapping.md`.

## Bilingual fields — fixed pattern, do not deviate
Every translatable field is a pair of columns/properties, always in this
order and with this exact suffix pattern:
```
NameEn / NameAr
DescriptionEn / DescriptionAr
```
Never a single `Name` column, never a JSON blob for translations, never
`NameArabic` / `NameEnglish` (wrong suffix), never reordered. This
consistency is what lets the frontend map fields mechanically without
per-entity guesswork. Applies to: `Category`, `SecondaryService`,
`WhatsAppMessageTemplate`. Note `Video.Title` and `Video.Description` are
**not** translatable-public fields — they are admin-only, single-language
(Arabic, since the admin works in Arabic), used only for search matching.
Do not "fix" them into an En/Ar pair; that would be a scope change to the
Search Matching rule in the Requirements doc.

## Database
- Table names: PascalCase, plural (`Videos`, `Categories`,
  `VideoCategories` for the join table).
- Foreign keys: `<Entity>Id` (`VideoId`, `CategoryId`).
- Migration names: `<Ticket-or-Phase>_<ShortDescription>`
  (`Phase1_AddVideoCategoryJoin`).

## Routes
All lowercase, kebab-case, plural nouns:
`/api/v1/videos`, `/api/v1/secondary-services`,
`/api/v1/contact-submissions`. Exact casing must match the API Contract
document byte-for-byte — this is the one place where "looks close enough"
is not close enough, since the frontend is coded against the literal
string.
