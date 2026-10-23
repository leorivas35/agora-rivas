# TypeScript Documentation

## Data Types
### `unknown` vs `any`
Both `unknown` and `any` can hold values of any type, but they differ in how safely you can use those values:

- **`unknown`** is the safe counterpart of `any`. You can assign any value to it, but TypeScript requires you to narrow or assert its type before using it.
- **`any`** disables type checking for that value. You can access properties or call it without checks, which can hide errors.

```ts
let safe: unknown = "hello";
safe.toUpperCase(); // Error: narrow the type first

if (typeof safe === "string") {
  safe.toUpperCase(); // OK: safe is string here
}

let unchecked: any = "hello";
unchecked.toUpperCase(); // Allowed, but unsafe
unchecked.notARealMethod(); // Also allowed; may fail at runtime
```

Prefer `unknown` for values whose type you don’t yet know, such as data from an API or `JSON.parse`. Use `any` only when you intentionally want to opt out of type checking.
## TypeScript Narrowing

TypeScript narrowing is the process of refining a value's declared type as the
program checks it. Within a branch where a check succeeds, TypeScript uses the
more specific type to validate operations and provide better editor support.
Narrowing affects static type checking; it does not convert or validate values
at runtime.

### `typeof` checks

Use JavaScript's `typeof` operator to distinguish primitive types:

```ts
function format(value: string | number) {
  if (typeof value === "string") {
    return value.toUpperCase(); // value is string
  }

  return value.toFixed(2); // value is number
}
```

`typeof` returns strings such as `"string"`, `"number"`, `"boolean"`,
`"bigint"`, `"symbol"`, `"undefined"`, `"object"`, and `"function"`. Note that
`typeof null` is `"object"`, so a null check may be needed as well.

### Truthiness checks

Conditions such as `if (value)` remove falsy possibilities from a union:

```ts
function printLength(value: string | null) {
  if (value) {
    console.log(value.length); // value is string
  }
}
```

Be aware that truthiness also excludes valid values such as `0`, `""`, and
`false`. When those values must be retained, use an explicit check such as
`value !== undefined` or `value !== null`.

### Equality checks

Comparing values can narrow their types, including when both sides share a
literal type:

```ts
function describe(x: string | number, y: string | boolean) {
  if (x === y) {
    // Both x and y are string in this branch.
    return x.toUpperCase();
  }
}
```

Checks against `null` or `undefined` are also useful for excluding those
members from a union.

### `in` checks

The `in` operator tests whether a property exists and can distinguish object
types with different properties:

```ts
type Fish = { swim: () => void };
type Bird = { fly: () => void };

function move(animal: Fish | Bird) {
  if ("swim" in animal) {
    animal.swim(); // animal is Fish
  } else {
    animal.fly(); // animal is Bird
  }
}
```

The property must be accessible on the narrowed type; optional properties can
still appear in either branch.

### `instanceof` checks

For class instances, `instanceof` narrows to the matching class:

```ts
function logDate(value: Date | string) {
  if (value instanceof Date) {
    console.log(value.toISOString()); // value is Date
  } else {
    console.log(value.toUpperCase()); // value is string
  }
}
```

### Discriminated unions

A shared literal property, called a discriminant, makes related object variants
straightforward to narrow:

```ts
type Shape =
  | { kind: "circle"; radius: number }
  | { kind: "rectangle"; width: number; height: number };

function area(shape: Shape): number {
  switch (shape.kind) {
    case "circle":
      return Math.PI * shape.radius ** 2;
    case "rectangle":
      return shape.width * shape.height;
  }
}
```

Each `case` narrows `shape` to the corresponding variant. This pattern is
especially useful for modeling state, events, and API results.

### Control-flow analysis

TypeScript follows control flow through branches, early returns, and
assignments. For example, returning from a null-check branch leaves a
non-null value afterward:

```ts
function getName(user: { name: string } | null) {
  if (user === null) {
    return "Anonymous";
  }

  return user.name; // user is non-null here
}
```

The compiler can also update its understanding after assignments. A narrowed
variable may widen again if a later assignment gives it a different union
member.

### User-defined type predicates

When built-in checks are not enough, a predicate function can communicate a
narrowing rule. Its return type has the form `parameterName is Type`:

```ts
type Fish = { swim: () => void };
type Bird = { fly: () => void };

function isFish(animal: Fish | Bird): animal is Fish {
  return "swim" in animal;
}

function move(animal: Fish | Bird) {
  if (isFish(animal)) {
    animal.swim(); // animal is Fish
  } else {
    animal.fly(); // animal is Bird
  }
}
```

The predicate is a promise made by the function's implementation: TypeScript
checks how callers use the predicate, but does not prove that its runtime test
is correct.

### Exhaustiveness with `never`

For a discriminated union, assign the remaining value to `never` to have the
compiler flag unhandled variants when the union grows:

```ts
type Result =
  | { status: "success"; value: string }
  | { status: "error"; message: string };

function assertNever(value: never): never {
  throw new Error(`Unhandled value: ${value}`);
}

function render(result: Result): string {
  switch (result.status) {
    case "success":
      return result.value;
    case "error":
      return result.message;
    default:
      return assertNever(result);
  }
}
```

If another `Result` variant is added without a corresponding branch, the
argument to `assertNever` is no longer `never`, producing a type error.

### Narrowing and runtime validation

Type annotations and narrowing checks do not validate untrusted data. For
example, data parsed from JSON should be checked at runtime before being treated
as a particular application type. Use real checks or a runtime validation
library; a type assertion such as `value as Shape` only changes what the
compiler assumes.

---
---
## Assertion functions
[Assertion functions in TypeScript 3.7](https://www.typescriptlang.org/docs/handbook/release-notes/typescript-3-7.html#assertion-functions)

An assertion function is a function that throws an error if a condition is not met. Its return type is `asserts condition`, which tells TypeScript that the condition is true after the function returns:

```ts
function assertIsString(value: unknown): asserts value is string {
  if (typeof value !== "string") {
    throw new Error("Not a string");
  }
}

function greet(value: unknown) {
  assertIsString(value);
  // TypeScript knows `value` is a string here
  console.log(`Hello, ${value.toUpperCase()}`);
}
```

A function call is analyzed as an assertion call or never-returning call when:

1. The call occurs as a top-level expression statement
2. The call specifies a single identifier or a dotted sequence of identifiers for the function name
3. Each identifier in the function name references an entity with an explicit type
4. The function name resolves to a function type with an `asserts` return type or an explicit `never` return type annotation

An entity is considered to have an explicit type when it is declared as:

- A function, method, class, or namespace
- A variable, parameter, or property with an explicit type annotation

> **Note:** This rule exists so that control flow analysis of potential assertion calls doesn't circularly trigger further analysis.
