using BjjSM.Modules.Curriculum.Application;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Curriculum;

public sealed class DeleteTechniqueEndpoint : EndpointWithoutRequest<MonthlyPlanDto>
{
    private readonly IMonthlyPlanRepository _repository;

    public DeleteTechniqueEndpoint(IMonthlyPlanRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Delete("/api/v1/plans/{id:guid}/weeks/{weekNumber:int}/techniques/{techniqueId:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var planId = Route<Guid>("id");
        var weekNumber = Route<int>("weekNumber");
        var techniqueId = Route<Guid>("techniqueId");

        var plan = await _repository.GetByIdAsync(planId, ct);
        if (plan is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var removeResult = plan.RemoveTechnique(weekNumber, techniqueId);
        if (removeResult.IsFailure)
        {
            AddError(removeResult.Error);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await _repository.SaveChangesAsync(ct);
        await Send.OkAsync(GetMonthlyPlanEndpoint.MapToDto(plan), ct);
    }
}
