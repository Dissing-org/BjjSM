# Architectural Guidelines & Standards

This document establishes the architectural rules and project structure for **BjjSM**. All code, scaffolding, and refactoring must adhere strictly to these principles.

---

## 1. Technology Stack

* **Frontend:** Blazor WebAssembly (.NET 8/9 / C#)
* **Backend:** ASP.NET Core Web API (C#)
* **Architecture Pattern:** Clean Architecture + Modular Monolith

---

## 2. Core Architectural Principles

### 2.1. Modular Monolith
The solution is organized into cohesive, business-bounded **Modules** (e.g., `Memberships`, `Tournaments`, `Curriculum`, `Finance`, `Identity`) housed in a single deployable repository:
* **High Cohesion, Loose Coupling:** Each module encapsulates its own domain, application logic, and persistence entities.
* **Module Independence:** Modules must **NOT** directly reference internal classes or database contexts of other modules.
* **Cross-Module Communication:**
  * Direct communication must happen solely through well-defined public module interfaces/contracts (e.g., `IMembershipModule`, DTOs/Events).
  * Asynchronous cross-module events (e.g., MediatR notifications or in-memory domain events) should be preferred for decoupling side effects (e.g., when a tournament registration happens, the finance module reacts).
* **Shared Kernel:** Only truly generic cross-cutting concerns (e.g., base `Entity`, `Result<T>`, logging, pagination primitives) reside in a shared kernel. Never place module-specific logic in shared libraries.

### 2.2. Clean Architecture (Per Module / Layer)
Within each module or subsystem, enforce strict dependency inversion:

```
[ Domain ] <--- [ Application ] <--- [ Infrastructure ]
       ^              ^
        \            /
     [ Presentation / API ]
```

1. **Domain Layer (Innermost):**
   * Contains Domain Entities, Value Objects, Domain Events, Enums, and Repository Interfaces.
   * **Zero External Dependencies:** Must have no references to EF Core, ASP.NET Core, Blazor, or third-party infrastructure packages.
   * Enforce rich domain models with business invariants; avoid purely anemic models where possible.

2. **Application Layer:**
   * Contains Use Cases, Commands, Queries (CQRS pattern with MediatR), DTOs, and Validators (FluentValidation).
   * Orchestrates business workflows. Depends only on the Domain layer and abstract service interfaces.

3. **Infrastructure Layer:**
   * Contains Database contexts (EF Core), database migrations, external integrations (email, payment gateways), file storage, and concrete repository implementations.
   * Implements interfaces defined in Domain and Application layers.

4. **Presentation / Web API Layer:**
   * ASP.NET Core Controllers or Minimal APIs acting as thin HTTP adapters.
   * Responsibilities are strictly limited to routing, status code mapping, model validation, and forwarding requests to the Application layer (e.g., via MediatR). No business logic in endpoints.

---

## 3. Frontend Architecture: Blazor WebAssembly

1. **Component Separation & Single Responsibility:**
   * Split complex UI into **Smart/Container Components** (pages that fetch data, handle state, and dispatch actions) and **Dumb/Presentational Components** (reusable UI elements with `[Parameter]` inputs and `EventCallback` outputs).
   * Keep `.razor` markup clean. For components with substantial logic, use code-behind files (`ComponentName.razor.cs`).

2. **API & Data Access Encapsulation:**
   * Blazor components must **NEVER** instantiate `HttpClient` or make raw HTTP requests directly in `.razor` markup.
   * All API communication must be mediated through strongly-typed API client services (e.g., `ITournamentApiClient`, `IMemberApiClient`) registered in dependency injection.

3. **Brand & Design System Compliance:**
   * All Blazor markup and CSS styling must strictly comply with [`AGENTS.md`](file:///c:/Users/mathi/BjjSM/AGENTS.md) (Brand Blue `#20409A`, Brand Yellow `#FFEE00`, Insignia Black `#050505`, Crisp White `#FFFFFF`).
   * Prefer scoped CSS (`ComponentName.razor.css`) or central theme tokens over ad-hoc styles.

---

## 4. Coding & Solution Standards

* **C# Language Features:**
  * Nullable reference types (`<Nullable>enable</Nullable>`) enabled across all projects. No ignoring compiler nullability warnings without explicit justification.
  * Use C# records for DTOs, Commands, Queries, and Value Objects.
* **Result Pattern:**
  * Prefer functional error handling / Result pattern (e.g., `Result<T>` or `OneOf`) for expected domain and application errors instead of throwing costly exceptions for control flow.
* **Testing Strategy:**
  * **Unit Tests:** Focus on Domain entities, value objects, and Application handlers.
  * **Architecture Tests:** Use NetArchTest or ArchUnitNET to automatically enforce dependency direction rules (e.g., verify Domain has no dependencies on Infrastructure).
  * **Component Tests:** bUnit for Blazor components.
