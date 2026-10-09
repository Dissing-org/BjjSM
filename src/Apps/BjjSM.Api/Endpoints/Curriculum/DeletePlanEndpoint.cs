using BjjSM.Modules.Curriculum.Application;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Curriculum;

public sealed class DeletePlanEndpoint : EndpointWithoutRequest
{
    private readonly IMonthlyPlanRepository _repository;

    public DeletePlanEndpoint(IMonthlyPlanRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Delete("/api/v1/plans/{id:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var planId = Route<Guid>("id");
        var plan = await _repository.GetByIdAsync(planId, ct);
        if (plan is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await _repository.DeleteAsync(plan, ct);
        await _repository.SaveChangesAsync(ct);
        await Send.NoContentAsync(ct);
    }
}
