# Error Handling — media-portfolio-backend

## Universal response envelope
Every response, success or failure, follows the exact shape defined in
the API Contract §1.4. Never return a bare object or a bare array. Never
let the default ASP.NET Core `ProblemDetails` shape leak out — the global
exception middleware must always translate into the contract's envelope.

## Global exception middleware
One piece of middleware, registered once in `Program.cs`, catches every
unhandled exception and maps it to the error envelope + an appropriate
status code:
- `ValidationException` (FluentValidation) → `400`, one error entry per
  failed field, `code: "FIELD_VALIDATION_FAILED"`.
- `NotFoundException` (custom, thrown by handlers) → `404`,
  `code: "RESOURCE_NOT_FOUND"`.
- `UnauthorizedAccessException` → `401`.
- Anything unexpected → `500`, `code: "INTERNAL_SERVER_ERROR"`, and the
  real exception is logged server-side but never included in the response
  body (no stack traces to the client, ever — including in staging).

## Error codes are a fixed, contract-owned vocabulary
Do not invent a new `code` string inline in a handler. Every code used
anywhere in this backend must appear in the API Contract's error code
registry. If a handler needs a new failure case, add the code to the
contract document first (and flag it to the frontend dev), then implement
it — never the other way around.

## Business-rule failures are not exceptions for control flow
Prefer a `Result<T>` return type (success/failure with a typed reason)
from handlers for expected business-rule outcomes (e.g. "category name
already exists"), reserving thrown exceptions for genuinely exceptional
paths. This keeps the pipeline behaviors and tests predictable.
