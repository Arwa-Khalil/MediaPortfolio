# Definition of Done — media-portfolio-backend

A task from `task-plan-backend.md` is not done until all of the
following are true:

1. **Matches the API Contract exactly.** Route, method, request shape,
   response shape (including the envelope), status codes, and error
   codes match `API-Contract-v1.0.docx` byte-for-byte. If the
   implementation needs to differ, the contract is updated first (and
   the frontend dev notified), not the other way around.
2. **Unit tests exist for real logic.** Anything with a business rule
   (category deletion behavior, video draft/publish transitions, contact
   retention job, WhatsApp template placeholder resolution, analytics
   counter increments) has a Domain or Application-layer unit test. Pure
   pass-through CRUD doesn't need exhaustive unit tests, but does need
   an integration test (below).
3. **Integration test covers the happy path** for every new endpoint,
   hitting the real (test) database via `MediaPortfolio.API.IntegrationTests`.
4. **Validation covers every input field**, including the fields the
   frontend won't validate itself (never trust the client — see
   `security-and-auth.md`).
5. **No secrets committed.** Config reviewed against
   `docker-and-config.md` before merge.
6. **Migration included** if the schema changed, and it has been run
   against a clean local database successfully.
7. **CORS and auth verified** — protected endpoints reject an
   unauthenticated request with `401`; public endpoints work without a
   token.
8. **Builds and passes CI** (build + test pipeline) before merge to the
   branch other work builds on.

## Phase exit criteria
Each phase in `task-plan-backend.md` has its own "Exit criteria" line —
that is the phase-level Definition of Done, checked before starting the
next phase, not after everything is "probably fine."
