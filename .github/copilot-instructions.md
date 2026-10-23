# Project Guidelines

## Working in this repository
- Inspect relevant code and nearby tests before editing; follow established structure, naming, and conventions.
- Keep changes focused. Avoid unrelated refactoring, new dependencies, or architectural layers unless the task requires them.
- Keep API and UI responsibilities clear: enforce business rules on the backend, and treat API models as contracts. Update affected clients and tests when contracts change.
- When a change spans the .NET API and Angular app, update and verify both.

## .NET API
- Use the repository's configured .NET version and project conventions.
- Use asynchronous I/O and propagate cancellation tokens where supported.
- Validate untrusted input at API boundaries and return consistent, appropriate HTTP responses.
- Keep database queries efficient and retrieve only the data needed.
- Never log secrets, credentials, or sensitive personal data.

## Angular
- Use the established Angular version and project patterns.
- Keep components focused; place reusable data access and business logic in appropriate services.
- Represent loading, empty, and error states. Use semantic HTML and accessible labels, keyboard interactions, and focus behavior.
- Avoid unsafe HTML and never store secrets in frontend code.

## Tests, builds, and documentation
- Add or update tests for behavior changes, following nearby test patterns.
- Run relevant tests and builds before finishing. Report commands run and any checks that could not be completed.
- Update documentation when behavior, setup, or API usage changes.