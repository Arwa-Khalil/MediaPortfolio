# CQRS + MediatR Patterns — media-portfolio-backend

## Every use case is a Command or a Query
No exceptions, including trivial ones like "toggle a boolean." A
`PublishVideoCommand` is a command, not a `PATCH` handled inline in a
controller. This keeps controllers thin (parse request → dispatch →
shape response) and keeps every business rule unit-testable in isolation.

## Structure
```
Features/
  Catalog/
    Commands/
      CreateVideo/
        CreateVideoCommand.cs
        CreateVideoCommandHandler.cs
        CreateVideoCommandValidator.cs
      PublishVideo/
      UnpublishVideo/
      DeleteVideo/
      AssignVideoCategories/
      CreateCategory/
      DeleteCategory/           # see "Category deletion" below
    Queries/
      GetVideosByCategory/
      SearchVideos/
      GetVideoById/
      ListCategories/
```

## One handler per command/query, one file each
Do not group multiple handlers in one file even when they're short. The
one-feature-one-folder shape is what makes the codebase navigable for two
developers working in parallel across two repos.

## Pipeline behaviors (cross-cutting, registered once)
- `ValidationBehavior` — runs FluentValidation validators before the
  handler executes; short-circuits with a `400` shaped per the API
  Contract's error envelope on failure.
- `TransactionBehavior` — wraps every Command (not Query) in a single EF
  Core transaction.
- `LoggingBehavior` — structured log of command/query name + duration.

## Category deletion — explicit rule
Deleting a `Category` that still has videos assigned to it is **not**
a cascade delete of those videos. The category assignment (the join-table
row) is removed for that category, but videos remain — they simply lose
that one category tag. A video with zero remaining categories still
exists as a draft/published entity; it just won't appear in any category
listing until the admin assigns a new one. This must be reflected in the
`DeleteCategoryCommandHandler` and covered by a unit test — do not let an
AI-assisted edit "helpfully" turn this into a cascade delete.

## Query results and the response envelope
Queries return Application-layer DTOs, not EF Core entities. The
`API` layer maps these into the wire-level response shapes defined in the
API Contract — never serialize a Domain or EF entity directly.
