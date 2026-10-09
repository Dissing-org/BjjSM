using BjjSM.Modules.Curriculum.Application;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Curriculum;

public record UpdateWeeklyThemeRequest(string Theme);

public sealed class UpdateWeeklyThemeEndpoint : Endpoint<UpdateWeeklyThemeRequest, MonthlyPlanDto>
{
    private readonly IMonthlyPlanRepository _repository;

    public UpdateWeeklyThemeEndpoint(IMonthlyPlanRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Put("/api/v1/plans/{id:guid}/weeks/{weekNumber:int}/theme");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateWeeklyThemeRequest req, CancellationToken ct)
    {
        var planId = Route<Guid>("id");
        var weekNumber = Route<int>("weekNumber");

        var plan = await _repository.GetByIdAsync(planId, ct);
        if (plan is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var updateResult = plan.UpdateWeekTheme(weekNumber, req.Theme);
        if (updateResult.IsFailure)
        {
            AddError(updateResult.Error);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await _repository.SaveChangesAsync(ct);
        await Send.OkAsync(GetMonthlyPlanEndpoint.MapToDto(plan), ct);
    }
}
