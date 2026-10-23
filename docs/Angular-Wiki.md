# Angular Wiki

This wiki contains information and documentation related to Angular development.

## Table of Contents
- [Getting Started](#getting-started)
- [Folder Structure](#folder-structure)
- [Components](#components)
- [Services](#services)
- [Routing](#routing)
- [Forms](#forms)
- [HTTP Client](#http-client)
- [State Management](#state-management)
- [Testing](#testing)
- [Deployment](#deployment)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)
- [Performance Optimization](#performance-optimization)
- [Security](#security)

---
# Getting Started
## Create a new project
To create a new Angular project, you need to have Node.js and Angular CLI installed on your system. Once installed, you can use the Angular CLI to generate a new project with your preferred configuration.

References:
- Angular CLI documentation: https://angular.dev/cli
- Angular getting started guide: https://angular.dev/start
- Angular workspace configuration: https://angular.dev/guide/workspace-config

Example setup:

```bash
npm install -g @angular/cli
ng new my-project --style=scss
cd my-project
ng serve
```
### CLI Command to create a new Angular project

The command used to create a new Angular project is:

```bash
ng new my-project --style=scss
```

- `my-project` is the name of your project.
- `--style=scss` sets SCSS as the default styling format.

For a new Angular project, SCSS (SASS) is the most widely recommended and industry-standard choice.

Here is how the main options compare to help you decide based on your project's needs:

1. SCSS (SASS) — Recommended
Why choose it: It provides all standard CSS features plus powerful extensions like variables, mixins, nesting, functions, and file modularity (@use / @forward).

Angular Ecosystem Support: Most major Angular component libraries (like Angular Material, PrimeNG, and Bootstrap) are built using SCSS, making integration, theme customization, and global styling seamless.

Build Integration: Angular CLI natively compiles SCSS using standard tools without requiring extra setup.

2. Standard CSS
Why choose it: Modern CSS has evolved significantly, now offering native CSS variables (custom properties), native nesting, and modern layout systems (Flexbox and Grid).

When to use: Ideal for smaller applications, lightweight projects, or teams that want to avoid preprocessor build steps and stick purely to native browser standards.

3. Sass (Indented Syntax)
Why choose it: Uses indentation instead of curly braces {} and semicolons ;.

When to use: Only recommended if your team already prefers Python-like strict indentation syntaxes, though it is less common in the Angular community than SCSS.

4. Less
Why choose it: Similar preprocessor features to SCSS.

When to use: Generally not recommended for new Angular projects unless you are maintaining legacy codebases or integrating with legacy Less-based UI frameworks.

Alternative / Utility-First Approaches
If you prefer a utility-first workflow:

Tailwind CSS: You can pair Tailwind with either SCSS or standard CSS in Angular. Tailwind handles utility classes for component layouts, while your primary stylesheet (SCSS/CSS) manages global resets, custom typography, and third-party UI library overrides.

Recommendation
Unless you have a strict requirement for vanilla CSS, select SCSS when running ng new my-project. It offers the greatest flexibility, ecosystem compatibility, and maintainability for modern Angular applications.

---

## Folder Structure

When you create a new Angular project, the Angular CLI generates a standard folder structure. Here is an overview of the main folders and files:

- `src/app`: Contains the main application code, including components, services, and modules.
- `src/assets`: Stores static assets like images, fonts, and other files.
- `src/environments`: Contains environment-specific configuration files.
- `angular.json`: Angular workspace configuration file.
- `package.json`: Lists project dependencies and scripts.
- `tsconfig.json`: TypeScript configuration file.
- `README.md`: Project documentation file.

Understanding this structure helps you organize your code effectively and follow Angular best practices.

The industry best practice is a Feature-Driven Architecture combined with a clear separation of Core (global/singleton) and Shared (reusable UI) concerns.

src/
├── app/
│   ├── core/                  # Global singletons, guards, interceptors, services
│   │   ├── guards/            # Route guards (auth.guard.ts)
│   │   ├── interceptors/      # HTTP interceptors (auth.interceptor.ts)
│   │   ├── services/          # Global services (auth.service.ts, theme.service.ts)
│   │   └── models/            # Global data models/interfaces (user.model.ts)
│   │
│   ├── shared/                # Reusable UI components, directives, pipes
│   │   ├── components/        # Presentational components (button, card, modal)
│   │   ├── directives/        # Custom directives (autofocus.directive.ts)
│   │   ├── pipes/             # Custom pipes (format-date.pipe.ts)
│   │   └── ui/                # UI design system tokens or helpers
│   │
│   ├── features/              # Feature modules / domain-driven folders
│   │   ├── models/            # Global data models/interfaces (user.model.ts, product.model.ts)
│   │   ├── auth/              # Auth domain feature
│   │   │   ├── components/    # Feature-specific UI (login-form)
│   │   │   ├── pages/         # Route level page views (login-page)
│   │   │   ├── services/      # Feature-specific state/data services
│   │   │   └── auth.routes.ts # Feature route definitions
│   │   │
│   │   └── dashboard/         # Dashboard domain feature
│   │       ├── components/
│   │       ├── pages/
│   │       └── dashboard.routes.ts
│   │
│   ├── layout/                # Main app layout shells
│   │   ├── header/            # Header component
│   │   ├── sidebar/           # Sidebar navigation
│   │   └── footer/            # Footer component
│   │
│   ├── app.config.ts          # Application providers (router, http, animations)
│   ├── app.routes.ts          # Top-level route configuration
│   └── app.component.ts       # Root app container component
│
├── assets/                    # Static media, icons, and i18n JSON files
└── styles/                    # Global SCSS files (variables, mixins, themes)

Core Principles & Best Practices
1. Group by Feature (Domain-Driven)
Avoid legacy grouping by file type (e.g., placing all components in one global /components folder).

Keep components, services, sub-routes, and models specific to a business feature grouped inside app/features/<feature-name>/.

This makes features modular, easier to test, and ready for lazy loading.

2. Standalone-First Setup
Avoid using NgModules unless working with legacy dependencies.

Use app.routes.ts with loadChildren or loadComponent for lazy loading feature routes:

```ts
// app.routes.ts
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.authRoutes)
  },
  {
    path: 'dashboard',
    loadChildren: () => import('./features/dashboard/dashboard.routes').then(m => m.dashboardRoutes)
  }
];
```

3. Strict Separation of core vs shared
app/core/: Import only once at root level. Place application-wide singletons, authentication logic, HTTP interceptors, global state, and guards here.

app/shared/: Presentational components, UI controls, pipes, and directives that are reused across multiple features. Shared elements should be stateless whenever possible.

4. Clean Import Aliases (TypeScript Path Mapping)
Configure path aliases in your tsconfig.json to prevent long relative imports (../../../../):

```json
{
  "compilerOptions": {
    "baseUrl": "./",
    "paths": {
      "@core/*": ["src/app/core/*"],
      "@shared/*": ["src/app/shared/*"],
      "@features/*": ["src/app/features/*"],
      "@env/*": ["src/environments/*"]
    }
  }
}
```

---