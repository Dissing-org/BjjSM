# Wayfinder Map: Frontend Implementation & End-to-End API Wiring

## Destination

A complete, responsive multi-page Blazor WebAssembly frontend connected to ASP.NET Core Web API using FastEndpoints:
- Navigation bar adhering to Brand Design System (`#20409A` Blue, `#FFEE00` Yellow, `#050505` Black, `#FFFFFF` White).
- **Member Roster & Profile**: List members, view authentic `<BjjBeltBadge />`, create new students, award promotions, and view/edit graduation timelines.
- **Monthly Curriculum Planner**: Visual 4–5 week Kanban canvas with technique categorization, video drill links, and 1-click QuestPDF syllabus download.
- Full end-to-end integration via strongly typed API clients.

## Notes

- **Brand System:** Consult [`AGENTS.md`](file:///c:/Users/mathi/BjjSM/AGENTS.md)
- **Architecture Guidelines:** Consult [`.agent/rules/architecture.md`](file:///c:/Users/mathi/BjjSM/.agent/rules/architecture.md)
- **Coding Standards:** Consult [`.agent/rules/coding-standards.md`](file:///c:/Users/mathi/BjjSM/.agent/rules/coding-standards.md) (DDD, TDD, Clean Architecture)
- **ADR References:** [ADR 0004 (Blazor UI)](file:///c:/Users/mathi/BjjSM/docs/adr/0004-blazor-wasm-ui-hierarchy.md), [ADR 0005 (FastEndpoints)](file:///c:/Users/mathi/BjjSM/docs/adr/0005-fastendpoints-web-api.md)

---

## Decisions so far

- **Ticket 005: FastEndpoints REST API Implementation for Memberships & Curriculum (RESOLVED)**: Implemented 8 FastEndpoints in `BjjSM.Api` providing full REST endpoints for Member roster, creation, promotion, MonthlyPlan retrieval, creation, drill addition, and QuestPDF download.
- **Ticket 006: Strongly-Typed Blazor API Clients & DTO Contracts (RESOLVED)**: Created shared request/response models (`MemberDtos.cs`, `CurriculumDtos.cs`), client abstractions (`IMemberApiClient`, `ICurriculumApiClient`), robust implementations (`MemberApiClient`, `CurriculumApiClient`), and registered them in `BjjSM.Client/Program.cs`.
- **Ticket 007: Member Roster Page, Creation Form & Detail View (RESOLVED)**: Implemented `NavMenu.razor` and `MainLayout.razor` with Japanese martial arts brand theme, `MembersPage.razor` with search and belt filtering, `CreateMemberModal.razor` for student registration, and `MemberDetailPage.razor` + `PromoteMemberModal.razor` for promotion ceremony tracking.
- **Ticket 008: Monthly Curriculum Planner Kanban Board & PDF Download Button (RESOLVED)**: Implemented `CurriculumPlannerPage.razor` featuring program and month/year selector, interactive 4-week thematic Kanban columns, categorized drill cards (`Warmup`, `Technique`, `SituationalDrill`, `CoolDown`), `AddTechniqueModal.razor`, and 1-click QuestPDF syllabus streaming with client-side base64 download helper.
- **Ticket 009: Blazor WASM Global Error Boundary & Toast Notification Architecture (RESOLVED)**: Permanently suppressed the stock `#blazor-error-ui` bottom footer banner from `index.html` and `app.css`. Built `AppErrorBoundary.razor` adhering to brand aesthetics (`#20409A`, `#FFEE00`, `#050505`) with collapsible technical diagnostics, component recovery, and safe fallback.
- **Ticket 010: Fix Member Roster JSON Deserialization & Pure Domain Rank Boundary (RESOLVED)**: Introduced `RankDto` in both API and Client models with implicit conversion from `Rank`. Decoupled `Rank` Value Object from JSON deserialization restrictions, added component-level error isolation with retry buttons, and verified smooth Member Roster rendering.
- **Ticket 011: Unified Single-Team Monthly Curriculum & Weekly Technique Planning (RESOLVED)**: Adapted curriculum module to single-team academy reality (no belt/team divisions, all ranks learn the same monthly curriculum). Added FastEndpoints for updating monthly objectives and weekly themes, enabled inline editing on Kanban columns, and added support for dynamic 4 or 5-week months.

---

## Frontier (Open Decisions)

All primary frontend and API milestone tickets are resolved!

---

## Not yet specified (Fog of War)

* **Photo/Avatar Uploads**: Whether student avatars will be local files or Gravatar/URL-based.
* **Academy Program Configuration**: UI for adding new custom programs (e.g. "Morning Masters 40+ Gi") dynamically.

---

## Out of scope

* **Third-Party Payment Gateways**: Automated Stripe checkout.
* **Tournament Bracket Matchmaking**.
