# Ticket 011: Unified Single-Team Monthly Curriculum & Weekly Technique Planning

**Labels**: `wayfinder:prototype`, `blazor-ui`  
**Parent**: [Wayfinder Frontend Map](file:///c:/Users/mathi/BjjSM/.wayfinder/MAP_FRONTEND.md)  
**Status**: Resolved ✅

## Summary of Implementation

1. **Unified Single-Team Model**:
   - Eliminated program/belt dropdowns from the UI.
   - Tied curriculum planning to the canonical academy unified team (`ProgramId: 11111111-1111-1111-1111-111111111111`), ensuring all belt ranks share one unified syllabus.
   - Prominently displays the `🥋 1 TEAM - Academy Unified Curriculum (All Belts & Ranks)` badge.

2. **Monthly Core Objective / Theme Inline Editing**:
   - Added `PUT /api/v1/plans/{id:guid}/objective` endpoint via FastEndpoints.
   - Added client method `ICurriculumApiClient.UpdateObjectiveAsync(...)`.
   - Enhanced the dark insignia monthly banner in [`CurriculumPlannerPage.razor`](file:///c:/Users/mathi/BjjSM/src/Apps/BjjSM.Client/Pages/CurriculumPlannerPage.razor) with inline edit/save/cancel controls.

3. **Weekly Sub-Theme Inline Editing**:
   - Added `PUT /api/v1/plans/{id:guid}/weeks/{weekNumber:int}/theme` endpoint via FastEndpoints.
   - Added client method `ICurriculumApiClient.UpdateWeekThemeAsync(...)`.
   - Enhanced Kanban week column headers with inline pencil toggles allowing instructors to edit and save weekly sub-themes directly on the board.

4. **Dynamic 4 or 5-Week Months**:
   - Added a duration selector on plan creation allowing instructors to choose between standard 4-week or extended 5-week months.
   - Responsive Kanban grid dynamically adapts to 4 or 5 week layouts.

5. **Deployment & Verification**:
   - All 20 domain and persistence unit tests passing.
   - Docker containers `bjjsm-api` and `bjjsm-web` successfully updated and verified live behind the Cloudflare tunnel (`https://bjjsm.dissing.dev/curriculum`).
