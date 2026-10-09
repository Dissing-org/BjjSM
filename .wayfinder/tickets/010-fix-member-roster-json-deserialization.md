# Ticket 010: Fix Member Roster JSON Deserialization & Pure Domain Rank Boundary

**Labels**: `wayfinder:grilling`, `architecture`  
**Parent**: [Wayfinder Frontend Map](file:///c:/Users/mathi/BjjSM/.wayfinder/MAP_FRONTEND.md)  
**Status**: Open (Claimable)

## Question

Why is the Member Roster failing when fetching practitioners, and what is the cleanest architectural pattern to serialize/deserialize `Rank` between FastEndpoints and Blazor WebAssembly?

### Root Cause Analysis:
- `Rank` is an encapsulated Domain Value Object with `private` constructors to enforce martial arts invariants.
- `System.Text.Json` in Blazor WASM cannot instantiate types with only `private` constructors without a public or `[JsonConstructor]` factory.
- When `GetMembersAsync()` calls `_http.GetFromJsonAsync<IReadOnlyList<MemberDto>>()`, deserialization throws an unhandled `NotSupportedException` / `JsonException`, which bubbles up and crashes the page.

### Architectural Options:
- **Option A (Pure Clean Architecture / DDD)**: Decouple Domain from DTOs by defining a dedicated `RankDto(BeltLadder Ladder, string BeltName, AdultBelt? Belt, KidsBelt? KidsBelt, int Stripes)` in `BjjSM.Client.Models` and `BjjSM.Api`. Domain `Rank` remains 100% pure with zero framework annotations.
- **Option B (Domain Annotation)**: Add `[JsonConstructor]` to a public constructor on `Rank` in `BjjSM.Modules.Memberships.Domain`. Less code, but leaks serialization concerns into the domain kernel.
- **Option C (Custom JsonConverter)**: Register a custom `RankJsonConverter` in `JsonSerializerOptions` in `MemberApiClient` and `FastEndpoints`.
