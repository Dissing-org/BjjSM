# ADR 0005: FastEndpoints for ASP.NET Core Web API Endpoints

- **Status:** Accepted
- **Date:** 2026-10-04
- **Context:** Web API Layer in BjjSM Modular Monolith (`BjjSM.Api`)

---

## Context and Problem Statement
The Web API layer needs to expose RESTful endpoints for both the Memberships and Curriculum modules to serve the Blazor WebAssembly client and external integrations. Traditional ASP.NET Core MVC controllers tend to become bloated "kitchen sink" classes violating Single Responsibility Principle (SRP).

## Decision Drivers
* Strict Single Responsibility Principle (SRP): One request, one response, one endpoint class (REPR pattern: Request-Endpoint-Response).
* Clean performance: FastEndpoints provides high-throughput, low-memory routing without the reflection overhead of traditional controller model binding.
* Clean Architecture alignment: Endpoints act purely as thin HTTP adapters invoking application use cases or repository operations.

---

## Decision Outcome
Chosen Option: **FastEndpoints NuGet Package (`FastEndpoints`)**.

### Endpoint Design & Architecture:
* **Memberships Endpoints**:
  * `GetMembersEndpoint`: `GET /api/v1/members`
  * `GetMemberByIdEndpoint`: `GET /api/v1/members/{id}`
  * `CreateMemberEndpoint`: `POST /api/v1/members`
  * `PromoteMemberEndpoint`: `POST /api/v1/members/{id}/promote`
  * `CorrectGraduationEndpoint`: `PUT /api/v1/members/{id}/graduations/{graduationId}`
  * `RemoveGraduationEndpoint`: `DELETE /api/v1/members/{id}/graduations/{graduationId}`
* **Curriculum Endpoints**:
  * `GetMonthlyPlanEndpoint`: `GET /api/v1/plans` (query params: `academyId`, `programId`, `year`, `month`)
  * `CreateMonthlyPlanEndpoint`: `POST /api/v1/plans`
  * `UpdateWeekThemeEndpoint`: `PUT /api/v1/plans/{id}/weeks/{weekNumber}/theme`
  * `AddTechniqueEndpoint`: `POST /api/v1/plans/{id}/weeks/{weekNumber}/techniques`
  * `PublishPlanEndpoint`: `POST /api/v1/plans/{id}/publish`
  * `DownloadPlanPdfEndpoint`: `GET /api/v1/plans/{id}/pdf` (returns `application/pdf` binary stream)

---

## Consequences
* **Positive:** High cohesion and modular organization; each use case is completely isolated in its own file.
* **Positive:** Built-in FluentValidation integration and automatic OpenAPI/Swagger generation.
* **Negative:** Introduces the `FastEndpoints` dependency into `BjjSM.Api`.
