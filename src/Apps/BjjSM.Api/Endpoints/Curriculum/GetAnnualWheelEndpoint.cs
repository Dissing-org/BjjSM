using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Domain;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Curriculum;

public record AnnualWheelSlotDto(
    Guid Id,
    int Month,
    Guid ThemeId,
    string ThemeName,
    string Objective,
    bool IsInstantiated,
    Guid? MonthlyPlanId);

public record AnnualWheelDto(
    Guid Id,
    Guid AcademyId,
    Guid ProgramId,
    int Year,
    WheelStatus Status,
    DateTime CreatedAt,
    DateTime LastModifiedAt,
    IReadOnlyList<AnnualWheelSlotDto> Slots);

public record GetAnnualWheelRequest(
    int Year,
    Guid AcademyId,
    Guid ProgramId);

public sealed class GetAnnualWheelEndpoint : Endpoint<GetAnnualWheelRequest, AnnualWheelDto>
{
    private readonly IAnnualWheelService _wheelService;

    public GetAnnualWheelEndpoint(IAnnualWheelService wheelService)
    {
        _wheelService = wheelService;
    }

    public override void Configure()
    {
        Get("/api/curriculum/wheel");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAnnualWheelRequest req, CancellationToken ct)
    {
        var result = await _wheelService.GetOrGenerateWheelAsync(req.AcademyId, req.ProgramId, req.Year, ct);
        if (result.IsFailure)
        {
            AddError(result.Error);
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var dto = MapToDto(result.Value);
        await Send.OkAsync(dto, ct);
    }

    internal static AnnualWheelDto MapToDto(AnnualWheel wheel) => new(
        wheel.Id,
        wheel.AcademyId,
        wheel.ProgramId,
        wheel.Year,
        wheel.Status,
        wheel.CreatedAt,
        wheel.LastModifiedAt,
        wheel.Slots.OrderBy(s => s.Month).Select(s => new AnnualWheelSlotDto(
            s.Id,
            s.Month,
            s.ThemeId,
            s.ThemeName,
            s.Objective,
            s.IsInstantiated,
            s.MonthlyPlanId
        )).ToList()
    );
}
