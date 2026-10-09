# Ticket 006: Strongly-Typed Blazor API Clients & DTO Contracts

**Labels**: `wayfinder:task`, `blazor-client`  
**Parent**: [Wayfinder Frontend Map](file:///c:/Users/mathi/BjjSM/.wayfinder/MAP_FRONTEND.md)  
**Status**: Open (Claimable)

## Question

How should the strongly-typed API client abstractions (`IMemberApiClient`, `ICurriculumApiClient`) and shared DTOs be implemented in `BjjSM.Client` to communicate with the FastEndpoints backend?

### Tasks:
1. Define shared Request/Response DTO contracts:
   - `MemberDto`, `CreateMemberRequest`, `PromoteMemberRequest`
   - `MonthlyPlanDto`, `SavePlanRequest`, `AddTechniqueRequest`
2. Implement `MemberApiClient` and `CurriculumApiClient` using injected `HttpClient`.
3. Register services in `Program.cs` (`builder.Services.AddScoped<IMemberApiClient, MemberApiClient>()`, etc.).
