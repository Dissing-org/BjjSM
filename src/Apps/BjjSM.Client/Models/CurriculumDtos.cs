using BjjSM.Modules.Curriculum.Domain;

namespace BjjSM.Client.Models;

public record MonthlyPlanDto(
    Guid Id,
    Guid AcademyId,
    Guid ProgramId,
    int Year,
    int Month,
    string MonthlyObjective,
    PlanStatus Status,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    IReadOnlyList<WeeklyFocusDto> Weeks);

public record WeeklyFocusDto(
    Guid Id,
    int WeekNumber,
    string Theme,
    IReadOnlyList<TechniqueDrillDto> Techniques);

public record TechniqueDrillDto(
    Guid Id,
    string Title,
    TechniqueCategory Category,
    string? CoachingNotes,
    string? ReferenceUrl,
    int SortOrder);

public record CreateMonthlyPlanRequest(
    Guid AcademyId,
    Guid ProgramId,
    int Year,
    int Month,
    string Objective,
    Guid CreatedByInstructorId,
    int WeekCount);

public record AddTechniqueRequest(
    string Title,
    TechniqueCategory Category,
    string? CoachingNotes,
    string? ReferenceUrl,
    int SortOrder);

public record UpdateWeeklyThemeRequest(string Theme);

public record UpdateObjectiveRequest(string Objective);

public record UpdateTechniqueRequest(
    string Title,
    TechniqueCategory Category,
    string? CoachingNotes,
    string? ReferenceUrl,
    int SortOrder);
