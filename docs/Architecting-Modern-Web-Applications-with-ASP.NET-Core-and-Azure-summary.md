# Summary: *Architecting Modern Web Applications with ASP.NET Core and Azure*

## Table of Contents

- [1. Characteristics of modern web applications](#1-characteristics-of-modern-web-applications)
- [2. Architectural principles](#2-architectural-principles)
- [3. Common web application architectures](#3-common-web-application-architectures)
- [4. Client-side web technologies](#4-client-side-web-technologies)
- [5. Working with data](#5-working-with-data)
- [6. Testing ASP.NET Core applications](#6-testing-aspnet-core-applications)
- [7. Development process for Azure](#7-development-process-for-azure)
- [8. Azure hosting recommendations](#8-azure-hosting-recommendations)
- [9. Practical implementation checklist](#9-practical-implementation-checklist)
- [10. Core message](#10-core-message)


## 1. Characteristics of modern web applications

Modern applications must be available, secure, responsive, scalable, and usable across devices. ASP.NET Core supports these needs through a small runtime footprint, high throughput, modular NuGet-based composition, cross-platform execution, and first-class dependency injection.

Key characteristics:

- **Cloud-ready and scalable:** A low-memory, high-throughput application can serve more traffic for the same resources and can scale out when demand rises.
- **Cross-platform:** Applications run on Windows, Linux, and macOS, and can be packaged in Linux or Windows containers.
- **Modular and loosely coupled:** Depend on interfaces and small packages rather than concrete implementations and large frameworks.
- **Testable:** Dependency injection makes fakes practical in unit tests; `TestServer`/in-memory hosting enables fast functional tests.
- **Flexible UI support:** ASP.NET Core can combine server-rendered MVC or Razor Pages, APIs, SPA functionality, and Blazor in one application.
- **Simple delivery:** A monolith can be deployed as one unit and integrated with CI/CD through Azure DevOps or GitHub Actions.

The eShopOnWeb reference application is intentionally small: it demonstrates an online-store domain and the recommended organization without obscuring the architecture with business complexity.

[Back to Top](#table-of-contents)


## 2. Architectural principles

The architecture should optimize for long-term maintainability, not merely initial delivery speed.

- **Separation of concerns:** Keep business behavior separate from UI and infrastructure. The business model should not need to know how data is stored or how requests are rendered.
- **Encapsulation:** Expose stable contracts and protect internal state. Replace an implementation without breaking consumers as long as the contract remains valid.
- **Dependency inversion:** High-level policy and low-level implementation should both depend on abstractions. Runtime flow may move outward to infrastructure, while compile-time dependencies should point inward.
- **Explicit dependencies:** Make a class's collaborators visible through constructor injection instead of service location or hidden global state.
- **Single responsibility:** A type or component should have one reason to change.
- **Persistence ignorance:** Avoid forcing domain entities to inherit persistence-specific base classes, implement persistence interfaces, use persistence attributes, or otherwise encode database requirements into the domain model.
- **Bounded contexts:** Split large problem domains into independent conceptual modules, each with its own model and ideally its own data store. Integrate contexts through APIs or messages, not a shared database.

[Back to Top](#table-of-contents)


## 3. Common web application architectures

A monolith is a single deployable unit whose core behavior runs in one process. It can still be internally modular and can scale horizontally by running multiple instances. A one-project application is a reasonable starting point for a small system, but folder-only separation becomes hard to maintain as the codebase grows.

Traditional N-layer designs separate presentation, business, and data access. This improves organization but can deteriorate if business logic leaks into the UI or data layer, or if the layers create unnecessary coupling.

The preferred model is **Clean Architecture**:

1. Put domain entities, value objects, domain events, and business rules at the center.
2. Put application use cases, interfaces, and orchestration around the domain.
3. Put UI, persistence, web frameworks, file systems, external services, and other implementation details at the outside.
4. Enforce inward compile-time dependencies; infrastructure implements interfaces defined by the inner layers.

This arrangement permits testing business behavior without a database or web server, supports alternate infrastructure implementations, and avoids letting EF Core or MVC shape the domain. Organize the code so dependency direction is clear, and consider grouping UI code by feature when that makes related work easier to find.

Containers do not require microservices. A modular monolith can be packaged in a single container to obtain consistent deployment and environmental isolation without distributed-system complexity.

[Back to Top](#table-of-contents)       

## 4. Client-side web technologies

Keep HTML (content and structure), CSS (presentation), and JavaScript (behavior) separate. Favor reusable classes and external stylesheets rather than inline styles and element-specific rules. Sass or LESS can reduce CSS duplication through variables and language features.

JavaScript frameworks such as Angular, React, and Vue can support rich browser applications, but choosing one creates a long-lived dependency. Evaluate:

- Team familiarity, including TypeScript where applicable.
- Whether the framework's conventions suit the application.
- Required ecosystem capabilities and library quality.
- Documentation, community health, release cadence, and support options.

Use a framework only when the interaction requirements justify its complexity. Blazor is an alternative for teams that want rich UI composition while remaining primarily in .NET and Razor.

[Back to Top](#table-of-contents)


### 4.1 Application structure and dependencies

Keep controllers and page models thin: they should translate HTTP requests into application work and select responses, not contain core business rules. Register dependencies in the composition root (`Program.cs`), request them through constructors, and depend on abstractions where infrastructure may vary.

Use feature organization when it makes behavior easier to discover: colocate the views, models, handlers, and related code for a feature rather than scattering them solely by technical type. Handle cross-cutting concerns such as authorization, logging, exception handling, caching, and validation centrally through middleware, filters, or dedicated services.

### 4.2 Hosting and deployment

Kestrel is ASP.NET Core's cross-platform web server. Production deployments commonly place it behind a reverse proxy that provides TLS termination and related edge concerns. A containerized app commonly runs Kestrel in the container behind a reverse proxy. Azure Application Gateway can supply load balancing, TLS options, routing, diagnostics, WebSocket support, and a web application firewall.

## 5. Working with data

Treat data access as infrastructure. Keep its implementation outside the domain and application core so the system remains testable and can evolve.

For new applications using relational data, the book recommends **Entity Framework Core**. Define a `DbContext` with `DbSet<TEntity>` collections, configure it through dependency injection, and use LINQ for queries. Use migrations to evolve schema deliberately. Configure logging when diagnosing the SQL generated by EF Core.

Choose persistence based on the problem rather than habit:

- **EF Core:** Default choice for most relational application development.
- **Micro-ORM or raw SQL:** Consider when very specific query control, performance characteristics, or existing conventions justify it.
- **SQL:** Strong fit for relational integrity, transactions, and well-understood querying.
- **NoSQL/Cosmos DB:** Consider for appropriate document, scale, availability, or data-model requirements; do not use it merely because it is cloud-native.

Use caching deliberately:

- Response caching is useful when a complete HTTP response can safely be reused.
- Data caching is useful for expensive or frequently read data, but requires an explicit invalidation/expiration strategy.

For Blazor WebAssembly clients, centralize `HttpClient` configuration, verify responses, and attach authentication tokens safely. Shared DTO projects can prevent server and client contracts from drifting.

[Back to Top](#table-of-contents)

## 6. Testing ASP.NET Core applications

Testing is an architectural outcome: code with explicit dependencies and separated concerns can be tested automatically at the appropriate level.

| Test type | Purpose | Characteristics |
|---|---|---|
| Unit test | Verify one unit of application logic | In-process, no filesystem/network/database, very fast, numerous |
| Integration test | Verify components and infrastructure work together | More setup and slower; use when a unit test cannot prove the behavior |
| Functional test | Verify user-visible requirements through the application stack | Tests routes, middleware, filters, binding, and output from the user's perspective |

Follow the testing pyramid: many fast unit tests, fewer integration tests, and only the functional tests needed to establish confidence in essential flows. Avoid using slow integration tests to prove logic that can be covered by a unit test.

ASP.NET Core's test host allows functional tests to exercise a complete MVC/Razor Pages stack in memory. This offers stronger coverage than isolated tests without requiring a deployed server or fragile network configuration.

[Back to Top](#table-of-contents)

## 7. Development process for Azure

Develop locally with either Visual Studio or Visual Studio Code plus the .NET CLI. Keep application code in source control, run automated tests, and automate build and deployment through Azure DevOps or GitHub Actions.

The recommended delivery loop is:

1. Build and test locally.
2. Push a change to source control.
3. Have CI build, test, and package the application.
4. Deploy through a release/CD pipeline, normally to a staging environment before production.
5. Run on Azure App Service or the chosen hosting platform.
6. Monitor health, exceptions, requests, performance, logs, and usage through Application Insights.

The key point is that Azure deployment should be a repeatable pipeline, not a manual publishing procedure.

[Back to Top](#table-of-contents)

## 8. Azure hosting recommendations

Choose the least operationally complex Azure service that satisfies the application's needs.

| Hosting option | Recommended use |
|---|---|
| Azure App Service Web Apps | Default for most web applications; managed PaaS, integrated deployment, scaling, load balancing, and availability |
| App Service for Containers | Containerized applications that still benefit from App Service management |
| Azure Kubernetes Service (AKS) | Microservices or advanced container orchestration needs |
| Azure Virtual Machines | Cases requiring operating-system or machine-level control |

Azure App Service is the normal recommendation for monolithic ASP.NET Core applications, including many containerized ones. It reduces infrastructure work and integrates deployment, scaling, load balancing, diagnostics, security, and Azure services. Use AKS only when container orchestration and service-level complexity are real requirements, not as a default modernization step.

Select managed services for supporting processes and data where possible. Design the deployed architecture around reliability, scalability, secure access, monitoring, and operational simplicity.

[Back to Top](#table-of-contents)

## 9. Practical implementation checklist

- Start with a modular monolith unless independent deployment and distributed ownership are proven needs.
- Put domain and application rules in inner projects; place EF Core, Azure SDKs, HTTP clients, and UI frameworks at the edges.
- Define interfaces in the inner layers and implement them in infrastructure.
- Use Razor Pages for new page-centric UI; use controllers for APIs and suitable MVC migration scenarios.
- Keep controllers, page models, and views focused on HTTP/UI concerns.
- Use EF Core for typical relational persistence and hide persistence details behind application-facing abstractions where appropriate.
- Build a test pyramid and protect key user journeys with in-memory functional tests.
- Automate builds, tests, deployment, and monitoring from source control.
- Default to Azure App Service; add containers, AKS, Application Gateway, or VMs only for specific justified requirements.

[Back to Top](#table-of-contents)

## 10. Core message

[Back to Top](#table-of-contents)

The book's consistent recommendation is to favor **simple, modular, testable, and operable designs**. ASP.NET Core and Azure provide many advanced options, but the right starting point for most systems is a cleanly organized monolith with explicit dependencies, automated validation, and managed cloud hosting. Complexity should be introduced only in response to concrete product, scale, or operational needs.
