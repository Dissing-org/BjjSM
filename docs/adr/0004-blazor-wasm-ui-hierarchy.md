# ADR 0004: Blazor WebAssembly Component Hierarchy and API Client Architecture

- **Status:** Accepted
- **Date:** 2026-10-04
- **Context:** Frontend Architecture in BjjSM (Blazor WebAssembly .NET 8/9)

---

## Context and Problem Statement
The frontend client must provide an intuitive, high-performance user experience for both students and coaches. It must cleanly display members' belt ranks and graduation timelines, support interactive curriculum planning with weekly technique authoring, enable 1-click PDF syllabus downloads, and adhere to strict clean frontend practices (Smart/Dumb component separation, no raw `HttpClient` in `.razor`, and strict brand palette integration).

## Decision Drivers
* Clean Architecture for Frontend: Smart (container/stateful) vs. Dumb (reusable/presentational) component separation.
* Interface Segregation: Strongly-typed API client abstractions registered via dependency injection (`IMemberApiClient`, `ICurriculumApiClient`).
* Brand Design System: Brand Blue (`#20409A`), Brand Yellow (`#FFEE00`), Insignia Black (`#050505`), Crisp White (`#FFFFFF`).
* Visual Authenticity: Custom CSS martial arts belt rendering component (`<BjjBeltBadge />`).

---

## Decision Outcome
Chosen Option: **Separated Component Tree with Dedicated Smart Pages, Dumb Presentation Elements, and Segregated API Clients**.

### 1. Component Architecture & Tree

```
App.razor
├── MainLayout.razor (Brand Blue top navbar, Crisp White canvas, Insignia Black branding)
│
├── Pages/Members/
│   ├── MemberListPage.razor (Smart)
│   │   ├── MemberFilterToolbar.razor (Dumb)
│   │   └── MemberTable.razor (Dumb)
│   │       └── BjjBeltBadge.razor (Dumb - authentic CSS belt strap + sleeve bar + stripes)
│   │
│   └── MemberDetailPage.razor (Smart)
│       ├── MemberHeaderCard.razor (Dumb - photo/avatar, roles, current rank badge)
│       ├── GraduationTimeline.razor (Dumb - chronological cards of past belts/stripes)
│       │   └── GraduationItem.razor (Dumb - rank badge, awarded date, instructor, notes, edit/delete actions)
│       └── PromoteMemberModal.razor (Dumb/Form - rank selector, date picker, notes, submit)
│
└── Pages/Curriculum/
    └── CurriculumPlannerPage.razor (Smart)
        ├── PlannerHeader.razor (Dumb - program dropdown, month picker, status badge, action buttons)
        │   ├── PublishButton.razor (Dumb)
        │   └── PdfDownloadButton.razor (Dumb - streams PDF blob from ICurriculumApiClient)
        ├── ObjectiveCard.razor (Dumb - editable monthly learning objective)
        └── WeeklyCanvas.razor (Dumb - responsive 4-5 column Kanban-style layout)
            └── WeekColumnCard.razor (Dumb)
                ├── WeekHeader.razor (Dumb - week number pill, editable theme title)
                ├── TechniqueList.razor (Dumb - ordered technique cards)
                │   └── TechniqueCard.razor (Dumb - category chip, title, notes, video link, action menu)
                └── AddTechniqueModal.razor (Dumb/Form - modal to add or edit drill/technique)
```

---

### 2. Authentic Martial Arts Belt Component (`<BjjBeltBadge />`)
A reusable presentational component rendering genuine BJJ rank styling:
* **Belt Strap**: Colored ribbon (White, Blue, Purple, Brown, Black, etc.).
* **Rank Sleeve Bar**:
  * Black sleeve bar on belts from White to Brown.
  * Red sleeve bar on Black belts (instructor bar) with white end borders.
* **Stripe Degrees**:
  * White rank stripes (0 to 4) rendered as crisp vertical bars within the sleeve bar.
  * Gold/Yellow stripes for high-degree black belts.
* **Accessibility**: Standard `aria-label="Adult Blue Belt with 2 stripes"` and tooltip for screen readers.

---

### 3. Strongly-Typed API Client Abstractions

All HTTP communication is strictly encapsulated inside service interfaces registered in `Program.cs`. Razor components receive clients via `@inject`:

#### A. `IMemberApiClient`
```csharp
public interface IMemberApiClient
{
    Task<IReadOnlyList<MemberSummaryDto>> GetMembersAsync(AcademyId academyId, CancellationToken ct = default);
    Task<MemberDetailDto?> GetMemberByIdAsync(PractitionerId id, CancellationToken ct = default);
    Task<Result> PromoteMemberAsync(PromoteMemberRequest request, CancellationToken ct = default);
    Task<Result> CorrectGraduationAsync(CorrectGraduationRequest request, CancellationToken ct = default);
    Task<Result> RemoveGraduationAsync(PractitionerId memberId, GraduationId graduationId, CancellationToken ct = default);
}
```

#### B. `ICurriculumApiClient`
```csharp
public interface ICurriculumApiClient
{
    Task<MonthlyPlanDto?> GetPlanAsync(AcademyId academyId, ProgramId programId, int year, int month, CancellationToken ct = default);
    Task<Result<MonthlyPlanDto>> SavePlanAsync(SaveMonthlyPlanRequest request, CancellationToken ct = default);
    Task<Result> PublishPlanAsync(MonthlyPlanId planId, CancellationToken ct = default);
    Task<byte[]> DownloadPlanPdfAsync(MonthlyPlanId planId, CancellationToken ct = default);
}
```

---

### 4. File-Behind & Styling Convention
* Complex components isolate C# logic in code-behind files (`ComponentName.razor.cs`).
* Component-specific CSS is isolated in scoped files (`ComponentName.razor.css`).
* Brand theme variables (`--primary: #20409A`, `--accent: #FFEE00`, `--foreground: #050505`) are imported globally in `wwwroot/css/app.css`.

---

## Consequences
* **Positive:** High component reusability and testability via `bUnit`.
* **Positive:** Complete isolation from raw HTTP transport details in UI markup.
* **Positive:** Clear, responsive user interface reflecting authentic BJJ culture and the Japanese martial arts brand theme.
