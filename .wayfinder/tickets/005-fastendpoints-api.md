# Ticket 005: FastEndpoints REST API Implementation for Memberships & Curriculum

**Labels**: `wayfinder:task`, `fastendpoints-api`  
**Parent**: [Wayfinder Frontend Map](file:///c:/Users/mathi/BjjSM/.wayfinder/MAP_FRONTEND.md)  
**Status**: Claimed (In Progress)  
**Assignee**: Agent & User Pairing


## Question

How should the FastEndpoints request/response handlers be structured in `BjjSM.Api` to expose the Memberships and Curriculum persistence services, and verify them via automated integration tests?

### Tasks:
1. Install `FastEndpoints` into `BjjSM.Api`.
2. Configure `MembershipsDbContext` and `CurriculumDbContext` in `Program.cs` with SQLite connection strings.
3. Implement Memberships endpoints:
   - `GET /api/v1/members`
   - `GET /api/v1/members/{id}`
   - `POST /api/v1/members` (create member with initial rank)
   - `POST /api/v1/members/{id}/promote`
   - `DELETE /api/v1/members/{id}/graduations/{graduationId}`
4. Implement Curriculum endpoints:
   - `GET /api/v1/plans`
   - `POST /api/v1/plans` (create draft)
   - `PUT /api/v1/plans/{id}/weeks/{weekNumber}/theme`
   - `POST /api/v1/plans/{id}/weeks/{weekNumber}/techniques`
   - `POST /api/v1/plans/{id}/publish`
   - `GET /api/v1/plans/{id}/pdf` (returns QuestPDF syllabus stream)
