using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Domain;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Curriculum;

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

public record GetPlanRequest(
    Guid AcademyId,
    Guid ProgramId,
    int Year,
    int Month);

public sealed class GetMonthlyPlanEndpoint : Endpoint<GetPlanRequest, MonthlyPlanDto?>
{
    private readonly IMonthlyPlanRepository _repository;

    public GetMonthlyPlanEndpoint(IMonthlyPlanRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Get("/api/v1/plans");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetPlanRequest req, CancellationToken ct)
    {
        var plan = await _repository.GetByMonthAsync(req.AcademyId, req.ProgramId, req.Year, req.Month, ct);
        if (plan is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var dto = MapToDto(plan);
        await Send.OkAsync(dto, ct);
    }

    internal static MonthlyPlanDto MapToDto(MonthlyPlan plan) => new(
        plan.Id,
        plan.AcademyId,
        plan.ProgramId,
        plan.Year,
        plan.Month,
        plan.MonthlyObjective,
        plan.Status,
        plan.CreatedAt,
        plan.PublishedAt,
        plan.Weeks.OrderBy(w => w.WeekNumber).Select(w => new WeeklyFocusDto(
            w.Id,
            w.WeekNumber,
            w.Theme,
            w.Techniques.OrderBy(t => t.SortOrder).Select(t => new TechniqueDrillDto(
                t.Id,
                t.Title,
                t.Category,
                t.CoachingNotes,
                t.ReferenceUrl,
                t.SortOrder
            )).ToList()
        )).ToList()
    );
}
