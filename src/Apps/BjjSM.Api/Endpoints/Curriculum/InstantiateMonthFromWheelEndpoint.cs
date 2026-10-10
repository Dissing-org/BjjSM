using BjjSM.Modules.Curriculum.Application;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Curriculum;

public record InstantiateMonthRequest(
    Guid WheelId,
    int Month,
    Guid InstructorId);

public sealed class InstantiateMonthFromWheelEndpoint : Endpoint<InstantiateMonthRequest, MonthlyPlanDto>
{
    private readonly IAnnualWheelService _wheelService;

    public InstantiateMonthFromWheelEndpoint(IAnnualWheelService wheelService)
    {
        _wheelService = wheelService;
    }

    public override void Configure()
    {
        Post("/api/curriculum/wheel/{wheelId}/months/{month}/instantiate");
        AllowAnonymous();
    }

    public override async Task HandleAsync(InstantiateMonthRequest req, CancellationToken ct)
    {
        var result = await _wheelService.InstantiateMonthPlanAsync(req.WheelId, req.Month, req.InstructorId, ct);
        if (result.IsFailure)
        {
            AddError(result.Error);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var dto = GetMonthlyPlanEndpoint.MapToDto(result.Value);
        await Send.OkAsync(dto, ct);
    }
}
