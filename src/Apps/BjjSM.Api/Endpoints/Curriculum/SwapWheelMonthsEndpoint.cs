using BjjSM.Modules.Curriculum.Application;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Curriculum;

public record SwapWheelMonthsRequest(
    Guid WheelId,
    int MonthA,
    int MonthB);

public sealed class SwapWheelMonthsEndpoint : Endpoint<SwapWheelMonthsRequest, AnnualWheelDto>
{
    private readonly IAnnualWheelService _wheelService;

    public SwapWheelMonthsEndpoint(IAnnualWheelService wheelService)
    {
        _wheelService = wheelService;
    }

    public override void Configure()
    {
        Post("/api/curriculum/wheel/{wheelId}/swap");
        AllowAnonymous();
    }

    public override async Task HandleAsync(SwapWheelMonthsRequest req, CancellationToken ct)
    {
        var result = await _wheelService.SwapMonthsAsync(req.WheelId, req.MonthA, req.MonthB, ct);
        if (result.IsFailure)
        {
            AddError(result.Error);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        await Send.OkAsync(GetAnnualWheelEndpoint.MapToDto(result.Value), ct);
    }
}
