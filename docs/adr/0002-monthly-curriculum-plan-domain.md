# ADR 0002: Monthly Curriculum Plan Aggregate and Lifecycle Design

- **Status:** Accepted
- **Date:** 2026-10-04
- **Context:** Curriculum & Planning Context in BjjSM Modular Monolith

---

## Context and Problem Statement
Instructors need to design and organize teaching syllabi on a monthly basis for specific academy programs (e.g., Adults Fundamentals, Advanced Gi, No-Gi, Kids). The plans require structured weekly breakdowns, detailed technique and drill annotations, reference links for students/coaches, and a clear lifecycle transition (`Draft -> Published -> Archived`) that supports minor mid-month refinements without revoking student visibility.

## Decision Drivers
* Rich Domain-Driven Design (DDD) encapsulation preserving invariant rules.
* Clean Architecture separation: Zero dependencies on persistence or presentation layers.
* Clear structure ready for deterministic PDF compilation (QuestPDF) and Blazor WASM editing.

---

## Decision Outcome
Chosen Design: **Encapsulated `MonthlyPlan` Aggregate Root with Nested `WeeklyFocus` and `TechniqueDrill` Entities**.

### Domain Entities & Value Objects:

### 1. `MonthlyPlan` Aggregate Root
* **Identity & Scope**:
  * `MonthlyPlanId Id`
  * `AcademyId AcademyId`
  * `ProgramId ProgramId`
  * `int Year` (e.g. 2026)
  * `int Month` (1..12)
* **Metadata & Objectives**:
  * `string MonthlyObjective` (e.g. "Guard Retention and Defensive Framing")
  * `PlanStatus Status` (`Draft`, `Published`, `Archived`)
  * `PractitionerId CreatedByInstructorId`
  * `DateTime CreatedAt`, `DateTime? PublishedAt`, `DateTime LastModifiedAt`
* **Internal Collections**:
  * `IReadOnlyCollection<WeeklyFocus> Weeks` (backed by private `List<WeeklyFocus> _weeks`)
* **Invariants & Methods**:
  * `Factory Create(AcademyId academyId, ProgramId programId, int year, int month, string objective, PractitionerId instructorId, int weekCount = 4)`:
    - Validates year (e.g., >= current year) and month (1..12).
    - Pre-populates `weekCount` (4 or 5) structured `WeeklyFocus` entities with default week numbers.
    - Sets initial status to `Draft`.
  * `UpdateObjective(string newObjective)`
  * `Publish(PractitionerId instructorId)`:
    - Validates that every week has at least a theme defined.
    - Transitions `Status` from `Draft` to `Published`.
    - Sets `PublishedAt = DateTime.UtcNow`.
    - Emits `MonthlyPlanPublishedDomainEvent`.
  * `Archive()`: Transitions `Status` to `Archived`.
  * `UpdateWeekTheme(int weekNumber, string theme)`
  * `AddOrUpdateTechnique(int weekNumber, TechniqueDrill technique)`
  * `RemoveTechnique(int weekNumber, TechniqueDrillId techniqueId)`

### 2. `WeeklyFocus` Entity
* **Identity**: `WeeklyFocusId Id`, `int WeekNumber` (1..5).
* **Properties**:
  * `string Theme` (e.g. "Week 1: Closed Guard Sweeps & Armbar Setup")
  * `IReadOnlyCollection<TechniqueDrill> Techniques` (backed by private `List<TechniqueDrill> _techniques`)

### 3. `TechniqueDrill` Entity / Value Object
* **Identity**: `TechniqueDrillId Id`.
* **Properties**:
  * `string Title` (e.g. "Scissor Sweep & Flower Sweep Transition")
  * `TechniqueCategory Category` (`Warmup`, `Technique`, `SituationalDrill`, `CoolDown`)
  * `string? CoachingNotes` (e.g. "Emphasize collar grip depth and shin alignment")
  * `string? ReferenceUrl` (validated URL string for video demonstration)
  * `int SortOrder`

---

## Consequences
* **Positive:** Aggregation guarantees that a plan always has a complete 4–5 week skeleton.
* **Positive:** Publishing is an explicit event suitable for triggering student notifications and cached PDF compilation.
* **Positive:** Accommodates in-place typo corrections and coaching note updates on published plans while keeping the audit timestamp updated.
* **Negative:** Modifying deep technique nodes requires routing mutations through the `MonthlyPlan` aggregate root.
