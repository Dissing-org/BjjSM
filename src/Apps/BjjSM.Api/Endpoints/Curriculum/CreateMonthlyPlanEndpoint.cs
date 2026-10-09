using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Domain;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Curriculum;

public record CreateMonthlyPlanRequest(
    Guid AcademyId,
    Guid ProgramId,
    int Year,
    int Month,
    string Objective,
    Guid CreatedByInstructorId,
    int WeekCount);

public sealed class CreateMonthlyPlanEndpoint : Endpoint<CreateMonthlyPlanRequest, MonthlyPlanDto>
{
    private readonly IMonthlyPlanRepository _repository;

    public CreateMonthlyPlanEndpoint(IMonthlyPlanRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Post("/api/v1/plans");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateMonthlyPlanRequest req, CancellationToken ct)
    {
        var existing = await _repository.GetByMonthAsync(req.AcademyId, req.ProgramId, req.Year, req.Month, ct);
        if (existing is not null)
        {
            AddError("A curriculum plan already exists for this program, month, and year.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var createResult = MonthlyPlan.Create(
            req.AcademyId == Guid.Empty ? Guid.NewGuid() : req.AcademyId,
            req.ProgramId == Guid.Empty ? Guid.NewGuid() : req.ProgramId,
            req.Year,
            req.Month,
            req.Objective,
            req.CreatedByInstructorId == Guid.Empty ? Guid.NewGuid() : req.CreatedByInstructorId,
            req.WeekCount == 0 ? 4 : req.WeekCount);

        if (createResult.IsFailure)
        {
            AddError(createResult.Error);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var plan = createResult.Value;
        await _repository.AddAsync(plan, ct);
        await _repository.SaveChangesAsync(ct);

        var dto = GetMonthlyPlanEndpoint.MapToDto(plan);
        await Send.CreatedAtAsync<GetMonthlyPlanEndpoint>(
            new GetPlanRequest(plan.AcademyId, plan.ProgramId, plan.Year, plan.Month),
            dto,
            cancellation: ct);
    }
}
