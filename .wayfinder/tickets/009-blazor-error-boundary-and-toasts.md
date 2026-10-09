# Ticket 009: Blazor WASM Global Error Boundary & Toast Notification Architecture

**Labels**: `wayfinder:grilling`, `blazor-ui`  
**Parent**: [Wayfinder Frontend Map](file:///c:/Users/mathi/BjjSM/.wayfinder/MAP_FRONTEND.md)  
**Status**: Open (Claimable)

## Question

How should application-wide error handling and user notification be structured in Blazor WASM to eliminate the default browser footer error banner (`#blazor-error-ui`) and provide graceful recovery?

### Key Considerations:
1. **Eliminate Default Error Bar**:
   - Suppress or hide the default `#blazor-error-ui` bottom banner from `index.html`.
   - Wrap the main router/layout in a custom `<ErrorBoundary>` component adhering to the Brand Design System (`#20409A` Blue, `#FFEE00` Yellow, `#050505` Black).
2. **Graceful Fault Isolation**:
   - Allow users to recover from uncaught exceptions with a "Try Again" or "Return to Dashboard" action without a hard browser page reload.
3. **Non-Breaking Operational Alerts**:
   - Provide an injectable notification service (`NotificationService` or inline alerts) for transient API failures (network drops, validation failures).
