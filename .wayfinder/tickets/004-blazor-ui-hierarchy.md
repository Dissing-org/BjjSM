# Ticket 004: Blazor WASM UI Component Hierarchy & View State

**Labels**: `wayfinder:prototype`, `blazor-architecture`  
**Parent**: [Wayfinder Map](file:///c:/Users/mathi/BjjSM/.wayfinder/MAP.md)  
**Status**: Closed  
**Assignee**: Agent & User Pairing  
**Resolution**: [ADR 0004: Blazor WebAssembly Component Hierarchy and API Client Architecture](file:///c:/Users/mathi/BjjSM/docs/adr/0004-blazor-wasm-ui-hierarchy.md)

## Question

What is the Blazor WebAssembly component tree, view-state management model, and strongly-typed API client structure for the Member Belt History and Monthly Curriculum Planner views?

## Resolution Summary
- Designed complete component tree separating Smart (Container) pages from Dumb (Presentational) reusable components.
- Specified custom authentic BJJ rank renderer `<BjjBeltBadge />` with belt strap, black/red sleeve bars, and rank stripes.
- Defined multi-column weekly Kanban canvas for curriculum authoring with inline technique modals and 1-click PDF download streaming.
- Established strongly-typed API clients `IMemberApiClient` and `ICurriculumApiClient` in DI, eliminating raw `HttpClient` usage in Razor files.
- Documented in [ADR 0004](file:///c:/Users/mathi/BjjSM/docs/adr/0004-blazor-wasm-ui-hierarchy.md).

