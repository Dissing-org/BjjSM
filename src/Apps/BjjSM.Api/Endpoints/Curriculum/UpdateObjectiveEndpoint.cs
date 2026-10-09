using BjjSM.Modules.Curriculum.Application;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Curriculum;

public record UpdateObjectiveRequest(string Objective);

public sealed class UpdateObjectiveEndpoint : Endpoint<UpdateObjectiveRequest, MonthlyPlanDto>
{
    private readonly IMonthlyPlanRepository _repository;

    public UpdateObjectiveEndpoint(IMonthlyPlanRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Put("/api/v1/plans/{id:guid}/objective");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateObjectiveRequest req, CancellationToken ct)
    {
        var planId = Route<Guid>("id");
        var plan = await _repository.GetByIdAsync(planId, ct);
        if (plan is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var updateResult = plan.UpdateObjective(req.Objective);
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
