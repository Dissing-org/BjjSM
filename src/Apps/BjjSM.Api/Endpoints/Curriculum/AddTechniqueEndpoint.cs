using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Domain;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Curriculum;

public record AddTechniqueRequest(
    string Title,
    TechniqueCategory Category,
    string? CoachingNotes,
    string? ReferenceUrl,
    int SortOrder);

public sealed class AddTechniqueEndpoint : Endpoint<AddTechniqueRequest, MonthlyPlanDto>
{
    private readonly IMonthlyPlanRepository _repository;

    public AddTechniqueEndpoint(IMonthlyPlanRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Post("/api/v1/plans/{id:guid}/weeks/{weekNumber:int}/techniques");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddTechniqueRequest req, CancellationToken ct)
    {
        var planId = Route<Guid>("id");
        var weekNumber = Route<int>("weekNumber");

        var plan = await _repository.GetByIdAsync(planId, ct);
        if (plan is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var technique = new TechniqueDrill(
            Guid.NewGuid(),
            req.Title,
            req.Category,
            req.CoachingNotes,
            req.ReferenceUrl,
            req.SortOrder);

        var addResult = plan.AddTechnique(weekNumber, technique);
        if (addResult.IsFailure)
        {
            AddError(addResult.Error);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await _repository.SaveChangesAsync(ct);
        await Send.OkAsync(GetMonthlyPlanEndpoint.MapToDto(plan), ct);
    }
}
