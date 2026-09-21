# API Contract Mapping — media-portfolio-backend

## The contract is the source of truth, this repo implements it
`API-Contract-v1.0.docx` (kept at this repo's root, identical copy in
`media-portfolio-frontend`) defines every route, request/response DTO,
enum, and error code. This backend's job is to make reality match that
document — never the reverse. If implementing a feature reveals the
contract is wrong, ambiguous, or missing something, stop and update the
contract (bump its version per its own versioning note) before writing
more code, and flag the change to whoever owns the frontend repo.

## Wire-boundary DTOs mirror the contract's JSON field names exactly
API-layer request/response classes use the exact casing and field names
from the contract (typically `camelCase` in JSON per §1 of the contract).
Application-layer DTOs (internal) may differ in shape/naming per this
repo's own conventions — the mapping between them happens explicitly at
the `API` layer, never by making the internal DTO "just also work" as the
wire DTO via shared inheritance. Two distinct types, one explicit mapping
step (AutoMapper or manual) — this is what stops an internal refactor
from silently breaking the contract.

## Enums
Every enum exposed over the API (`VideoStatus`, `ContactSubmissionStatus`)
is serialized as its **string** name (`"Draft"`, `"Published"`), never
its integer value — matches the contract's Domain Enumerations section
and avoids a silent break if enum member order ever changes.

## Versioning
Routes are prefixed `/api/v1/...`. A breaking change to any existing
route bumps to `/api/v2/...` per the contract's own versioning rules —
this backend never changes an existing v1 response shape in place once
the frontend repo is building against it.
