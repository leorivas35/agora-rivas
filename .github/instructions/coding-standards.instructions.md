---
applyTo: "**/*.{cs,csx,ts,tsx,js,jsx,mjs,cjs}"
---

# C#, TypeScript, and JavaScript Coding Standards

Follow the repository `.editorconfig` for editor-enforced formatting. These
instructions define the coding decisions that formatting alone cannot enforce.

## Shared conventions

- Use clear, intention-revealing names. Prefer complete words over
  abbreviations, except for well-known technical terms.
- Keep functions and classes focused on one responsibility. Extract a named
  helper when a block has a distinct purpose or is reused.
- Import only the symbols a file uses. Remove unused imports and prefer
  explicit named imports to wildcard imports where supported.
- Use braces for every conditional, loop, and `try`/`catch`/`finally` block,
  even when the body has a single statement.
- Do not suppress compiler, type-checking, or linting diagnostics without a
  precise justification and the smallest possible scope.

## C#

- Use `PascalCase` for namespaces, types, public and protected members, and
  methods. Prefix interfaces with `I`. Use `camelCase` for parameters, local
  variables, and private fields.
- Declare an explicit accessibility modifier for non-interface members. Use
  the narrowest accessibility that supports the intended API.
- Use `var` when the assigned type is apparent from the right-hand side; use
  an explicit type when it makes the code easier to understand or the type is
  not obvious.
- Keep nullable reference types accurate. Model optional values with `?`,
  validate external inputs at boundaries, and handle null before dereferencing.
  Do not use the null-forgiving (`!`) operator merely to silence warnings.
- Prefer guard clauses for invalid or unsupported inputs. Use `ArgumentNullException.ThrowIfNull`
  for required reference arguments when appropriate.
- Place `System` imports first, followed by other imports. Use file-scoped
  namespaces for new files unless a block-scoped namespace is required.
- Prefer immutable data: use `readonly` fields and `init` accessors when
  mutation is not required. Preserve asynchronous flows with `async` APIs and
  propagate cancellation tokens when supported.

## TypeScript and JavaScript

- Use `PascalCase` for classes, interfaces, types, enums, and Angular
  components; use `camelCase` for variables, functions, methods, and object
  properties; use `UPPER_SNAKE_CASE` only for true constants.
- Use single quotes for string literals, except when escaping would make a
  double-quoted string more readable.
- In TypeScript, preserve strict typing. Prefer inferred types when evident,
  explicit types at public boundaries, and `unknown` rather than `any` for
  untrusted or uncertain values. Narrow `unknown` before use.
- Treat `null` and `undefined` deliberately. Represent optional values in
  types, use optional chaining and nullish coalescing only when their fallback
  semantics are correct, and validate values before use.
- Prefer `const`; use `let` only for values that must be reassigned. Never use
  `var`.
- Prefer `===` and `!==` over loose equality. Avoid truthiness checks when
  valid values such as `0`, `false`, or an empty string must be distinguished
  from missing values.
- Use `async`/`await` for asynchronous control flow. Await promises that must
  complete, return promises from async APIs, and handle expected errors at the
  appropriate boundary.
