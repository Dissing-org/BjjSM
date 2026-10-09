# Ticket 001: Member & Graduation Aggregate Domain Design

**Labels**: `wayfinder:grilling`, `domain-modeling`  
**Parent**: [Wayfinder Map](file:///c:/Users/mathi/BjjSM/.wayfinder/MAP.md)  
**Status**: Closed  
**Assignee**: Agent & User Pairing  
**Resolution**: [ADR 0001: Practitioner Aggregate and Rank Domain Design](file:///c:/Users/mathi/BjjSM/docs/adr/0001-practitioner-aggregate-and-rank-domain.md)

## Question

How should the `Practitioner` (Member) aggregate root, `Rank` value object (Adult & Kids division), and `Graduation` entity/events be modeled in C# to satisfy DDD invariants, encapsulation, and correction rules?

## Resolution Summary
- Designed `Rank` immutable Value Object validating Adult & Kids belt ladders and stripe bounds (0-4 stripes, degrees for black/coral/red).
- Designed `Practitioner` aggregate root with an encapsulated `CurrentRank` and internal `_graduations` collection.
- Encapsulated `Promote()`, `CorrectGraduation()`, and `RemoveGraduation()` operations ensuring `CurrentRank` is automatically maintained and appropriate domain events (`PractitionerPromotedDomainEvent`, etc.) are raised.
- Documented in [ADR 0001](file:///c:/Users/mathi/BjjSM/docs/adr/0001-practitioner-aggregate-and-rank-domain.md).

