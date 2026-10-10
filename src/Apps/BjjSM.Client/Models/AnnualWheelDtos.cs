using BjjSM.Modules.Curriculum.Domain;

namespace BjjSM.Client.Models;

public record AnnualWheelSlotDto(
    Guid Id,
    int Month,
    Guid ThemeId,
    string ThemeName,
    string Objective,
    bool IsInstantiated,
    Guid? MonthlyPlanId);

public record AnnualWheelDto(
    Guid Id,
    Guid AcademyId,
    Guid ProgramId,
    int Year,
    WheelStatus Status,
    DateTime CreatedAt,
    DateTime LastModifiedAt,
    IReadOnlyList<AnnualWheelSlotDto> Slots);

public record CurriculumThemeDto(
    Guid Id,
    string Name,
    string Category,
    string DefaultObjective,
    IReadOnlyList<ThemeDrillTemplateDto> DefaultDrills);

public record ThemeDrillTemplateDto(
    Guid Id,
    string Title,
    TechniqueCategory Category,
    string? CoachingNotes,
    string? ReferenceUrl,
    int TargetWeekNumber,
    int SortOrder);

public record SwapMonthsRequest(int MonthA, int MonthB);

public record AssignThemeRequest(Guid ThemeId, string? CustomObjective);

public record InstantiateMonthRequest(Guid InstructorId);
