# Ticket 002: Monthly Curriculum Plan Aggregate & Lifecycle Design

**Labels**: `wayfinder:grilling`, `domain-modeling`  
**Parent**: [Wayfinder Map](file:///c:/Users/mathi/BjjSM/.wayfinder/MAP.md)  
**Status**: Closed  
**Assignee**: Agent & User Pairing  
**Resolution**: [ADR 0002: Monthly Curriculum Plan Aggregate and Lifecycle Design](file:///c:/Users/mathi/BjjSM/docs/adr/0002-monthly-curriculum-plan-domain.md)

## Question

How should the `MonthlyPlan` aggregate root and its children (`WeeklyFocus`, `TechniqueDrill`) be designed in C# Clean Architecture to support drafting, weekly breakdowns, technique annotations, and state transitions?

## Resolution Summary
- Designed `MonthlyPlan` aggregate root with composite identity `(AcademyId, ProgramId, Year, Month)` and explicit lifecycle states (`Draft`, `Published`, `Archived`).
- Automated 4–5 week skeleton initialization with nested `WeeklyFocus` entities.
- Rich `TechniqueDrill` models with categories (`Warmup`, `Technique`, `SituationalDrill`, `CoolDown`), coaching notes, sort orders, and reference video URLs.
- In-place minor modifications supported on published plans updating `LastModifiedAt` without unpublishing.
- Documented in [ADR 0002](file:///c:/Users/mathi/BjjSM/docs/adr/0002-monthly-curriculum-plan-domain.md).

