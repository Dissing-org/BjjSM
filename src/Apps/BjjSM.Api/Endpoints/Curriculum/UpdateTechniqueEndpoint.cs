using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Domain;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Curriculum;

public record UpdateTechniqueRequest(
    string Title,
    TechniqueCategory Category,
    string? CoachingNotes,
    string? ReferenceUrl,
    int SortOrder);

public sealed class UpdateTechniqueEndpoint : Endpoint<UpdateTechniqueRequest, MonthlyPlanDto>
{
    private readonly IMonthlyPlanRepository _repository;

    public UpdateTechniqueEndpoint(IMonthlyPlanRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Put("/api/v1/plans/{id:guid}/weeks/{weekNumber:int}/techniques/{techniqueId:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateTechniqueRequest req, CancellationToken ct)
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

        var updateResult = plan.UpdateTechnique(
            weekNumber,
            techniqueId,
            req.Title,
            req.Category,
            req.CoachingNotes,
            req.ReferenceUrl,
            req.SortOrder);
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
