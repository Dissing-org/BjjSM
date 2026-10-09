# Wayfinder Map: BJJ Student & Curriculum Management System

## Destination

A complete, production-ready specification and technical blueprint for **BjjSM**:
- Bounded contexts, Aggregate Roots, Value Objects, and Domain Events.
- REST API contracts & CQRS Handlers for Member/Graduation and Monthly Planning modules.
- Blazor WebAssembly component design & interaction flows adhering to brand theme.
- High-fidelity PDF document rendering template specification (QuestPDF).
- Ready for immediate TDD execution with Red-Green-Refactor gates.

## Notes

- **Domain Glossary:** Consult [`CONTEXT.md`](file:///c:/Users/mathi/BjjSM/CONTEXT.md)
- **Architecture Guidelines:** Consult [`.agent/rules/architecture.md`](file:///c:/Users/mathi/BjjSM/.agent/rules/architecture.md) (Modular Monolith, Clean Architecture, Blazor WASM, C# Web API)
- **Coding Standards & Pre-Push:** Consult [`.agent/rules/coding-standards.md`](file:///c:/Users/mathi/BjjSM/.agent/rules/coding-standards.md) (DDD, SOLID, DRY, TDD Red-Green-Refactor)
- **Brand Palette:** Consult [`AGENTS.md`](file:///c:/Users/mathi/BjjSM/AGENTS.md) (`#20409A` Blue, `#FFEE00` Yellow, `#050505` Black, `#FFFFFF` White)

---

## Decisions so far

- [[Ticket 001: Member & Graduation Aggregate Domain Design](file:///c:/Users/mathi/BjjSM/.wayfinder/tickets/001-member-graduation-domain.md)]: Practitioner aggregate encapsulates immutable Rank VO (Adult/Kids) and graduation timeline; maintains CurrentRank and emits promotion domain events. Recorded in [ADR 0001](file:///c:/Users/mathi/BjjSM/docs/adr/0001-practitioner-aggregate-and-rank-domain.md).
- [[Ticket 002: Monthly Curriculum Plan Aggregate & Lifecycle Design](file:///c:/Users/mathi/BjjSM/.wayfinder/tickets/002-curriculum-plan-domain.md)]: MonthlyPlan aggregate with (AcademyId, ProgramId, Year, Month), automatic 4-5 week skeleton, structured technique drills, and Draft->Published lifecycle. Recorded in [ADR 0002](file:///c:/Users/mathi/BjjSM/docs/adr/0002-monthly-curriculum-plan-domain.md).
- [[Ticket 003: QuestPDF Layout & Brand Styling Specification](file:///c:/Users/mathi/BjjSM/.wayfinder/tickets/003-questpdf-layout-spec.md)]: Single-page landscape A4 QuestPDF template strictly adhering to brand colors (#20409A Blue, #FFEE00 Yellow, #050505 Black), with embedded QR codes and video hyperlinks. Recorded in [ADR 0003](file:///c:/Users/mathi/BjjSM/docs/adr/0003-questpdf-layout-spec.md).
- [[Ticket 004: Blazor WASM UI Component Hierarchy & View State](file:///c:/Users/mathi/BjjSM/.wayfinder/tickets/004-blazor-ui-hierarchy.md)]: Smart/Dumb component tree, authentic `<BjjBeltBadge />` CSS rank renderer, weekly Kanban curriculum canvas, and strongly-typed API client abstractions. Recorded in [ADR 0004](file:///c:/Users/mathi/BjjSM/docs/adr/0004-blazor-wasm-ui-hierarchy.md).

---

## Frontier (Open Decisions)

*(Frontier is clear! All decision tickets for this destination have been resolved.)*






---

## Not yet specified (Fog of War)

* **Attendance & Mat Hours Integration:** How attendance tracking (if added later) ties into graduation eligibility criteria (e.g., minimum mat hours before 3rd stripe).
* **Multi-Gym Federation:** Cross-academy transfers (e.g., student moves from Affiliate Gym A to HQ Gym B).
* **Video/Media Hosting:** Whether curriculum drill reference links are external embeds (YouTube/Vimeo) or direct uploaded media assets.

---

## Out of scope

* **Payment Gateway & Automated Billing Subscriptions:** Stripe/recurring billing is excluded from this MVP effort; membership focuses on coaching, rank, and syllabus planning.
* **Competition Bracket Generation:** Tournament brackets and scorekeeping are distinct systems outside the academy student/curriculum scope.
