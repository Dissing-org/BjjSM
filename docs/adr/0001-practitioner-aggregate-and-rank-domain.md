# ADR 0001: Practitioner Aggregate and Rank Domain Design

- **Status:** Accepted
- **Date:** 2026-10-04
- **Context:** Member & Graduation Context in BjjSM Modular Monolith

---

## Context and Problem Statement
In a Brazilian Jiu-Jitsu academy, practitioners (students and instructors) progress through a structured belt and stripe hierarchy across two ladders (Adult and Junior). The system must record promotions accurately, maintain an immutable timeline while supporting corrections for human data-entry errors, and support transfer students who onboard with pre-existing ranks.

## Decision Drivers
* Strict Domain-Driven Design (DDD) with encapsulated invariants and no anemic models.
* Clean Architecture separation: Domain layer must have zero dependencies on EF Core, Web API, or Blazor.
* Practical academy realities: Accommodating transfer students joining at non-white belts and allowing coaches to correct miskeyed dates/stripes.

---

## Considered Options
1. **Event-Sourced Practitioner Model**: Compute current rank strictly by replaying graduation events.
2. **Encapsulated Aggregate Root with Cached CurrentRank & Navigation Collection**: Store an internal list of `Graduation` entities, maintain an explicit `CurrentRank` value object, and emit domain events upon promotions or corrections.
3. **Anemic CRUD Entity**: Expose public getters and setters on `Practitioner` and `Graduation`.

---

## Decision Outcome
Chosen Option: **Option 2 (Encapsulated Aggregate Root with Cached CurrentRank)**.

### Architectural & Domain Specifications:

### 1. `Rank` Value Object
* **Division Ladder**: `Adult` or `Kids`.
* **Belt Color**:
  * Adult: `White`, `Blue`, `Purple`, `Brown`, `Black`, `Coral`, `Red`.
  * Kids: `White`, `GreyWhite`, `Grey`, `GreyBlack`, `YellowWhite`, `Yellow`, `YellowBlack`, `OrangeWhite`, `Orange`, `OrangeBlack`, `GreenWhite`, `Green`, `GreenBlack`.
* **Stripes / Degrees**:
  * For standard belts: `0..4`.
  * For Adult Black belt: `0..6` degrees.
  * For Coral belt: `7..8` degrees.
  * For Red belt: `9..10` degrees.
* Invariants: Factory methods (`Rank.CreateAdult(...)`, `Rank.CreateKids(...)`) enforce stripe/degree bounds and return a `Result<Rank>` or throw domain exception on invalid values.

### 2. `Graduation` Entity
* Encapsulates: `GraduationId`, `AcademyId`, `Rank`, `AwardedAt` (DateTime), `AwardedBy` (InstructorId), and `Notes` (string?).
* Invariants:
  * Effective award date cannot be in the future.
  * `AwardedBy` must be a valid instructor identifier.
* Internal mutations: Protected/internal methods `UpdateDetails(...)` called solely by the aggregate root.

### 3. `Practitioner` Aggregate Root
* Encapsulates:
  * `PractitionerId Id`
  * `AcademyId AcademyId`
  * `string FirstName`, `string LastName`
  * `Email Email` (Value Object)
  * `PractitionerRole Roles` (Flag enum: `Student`, `Instructor`, `Admin`)
  * `Rank CurrentRank` (Value Object, private set)
  * `IReadOnlyCollection<Graduation> Graduations` (backed by private `List<Graduation> _graduations`)
* Methods:
  * `Promote(Rank newRank, DateTime awardedAt, PractitionerId awardedBy, string? notes)`:
    - Adds new `Graduation` entity.
    - Updates `CurrentRank = newRank`.
    - Emits `PractitionerPromotedDomainEvent`.
  * `CorrectGraduation(GraduationId id, Rank correctedRank, DateTime awardedAt, PractitionerId awardedBy, string? notes)`:
    - Finds entry, updates details.
    - Re-evaluates `CurrentRank` (latest by `AwardedAt`).
    - Emits `GraduationCorrectedDomainEvent`.
  * `RemoveGraduation(GraduationId id)`:
    - Removes entry.
    - Re-evaluates `CurrentRank` (latest by `AwardedAt`, or fallback to initial rank).
    - Emits `GraduationRemovedDomainEvent`.

---

## Consequences
* **Positive:** High cohesion and rich encapsulation; impossible to place the aggregate in an invalid state.
* **Positive:** Direct queryability of `CurrentRank` in EF Core without expensive aggregations or projections.
* **Positive:** Supports out-of-order data entry and onboarding transfer students cleanly.
* **Negative:** Aggregate must recalculate `CurrentRank` upon correction or deletion of historical records.
