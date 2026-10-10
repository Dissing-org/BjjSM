using BjjSM.Modules.Curriculum.Application;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Curriculum;

public record AssignWheelMonthThemeRequest(
    Guid WheelId,
    int Month,
    Guid ThemeId,
    string? CustomObjective);

public sealed class AssignWheelMonthThemeEndpoint : Endpoint<AssignWheelMonthThemeRequest, AnnualWheelDto>
{
    private readonly IAnnualWheelService _wheelService;

    public AssignWheelMonthThemeEndpoint(IAnnualWheelService wheelService)
    {
        _wheelService = wheelService;
    }

    public override void Configure()
    {
        Put("/api/curriculum/wheel/{wheelId}/months/{month}/theme");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AssignWheelMonthThemeRequest req, CancellationToken ct)
    {
        var result = await _wheelService.AssignThemeAsync(req.WheelId, req.Month, req.ThemeId, req.CustomObjective, ct);
        if (result.IsFailure)
        {
            AddError(result.Error);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await Send.OkAsync(GetAnnualWheelEndpoint.MapToDto(result.Value), ct);
    }
}
