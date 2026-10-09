# Ticket 008: Monthly Curriculum Planner Kanban Board & PDF Download Button

**Labels**: `wayfinder:prototype`, `blazor-ui`  
**Parent**: [Wayfinder Map](file:///c:/Users/mathi/BjjSM/.wayfinder/MAP_FRONTEND.md)  
**Status**: Open (Claimable)

## Question

How should the Monthly Curriculum Planner (`/curriculum`) Kanban-style board be laid out in Blazor WASM to enable intuitive technique authoring and 1-click PDF syllabus streaming?

### Tasks:
1. Build `CurriculumPlannerPage.razor`:
   - Program selector dropdown (Fundamentals, Advanced, No-Gi, Kids).
   - Month/Year selector with Status pill (`Draft` vs `Published`).
   - "Start Draft" button when no plan exists.
2. Build Weekly Kanban Canvas:
   - 4–5 week columns with editable theme input.
   - Technique drill cards with category pills (`Warmup`, `Technique`, `Drill`, `CoolDown`).
   - Modal to add new technique with title, notes, and video URL.
3. Build Actions:
   - "Publish Plan" button with validation.
   - "Download PDF Syllabus" button that invokes `ICurriculumApiClient.DownloadPlanPdfAsync` and streams the file directly to the browser.
