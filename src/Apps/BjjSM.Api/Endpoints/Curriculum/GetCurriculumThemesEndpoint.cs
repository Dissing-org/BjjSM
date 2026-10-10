using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Domain;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Curriculum;

public record ThemeDrillTemplateDto(
    Guid Id,
    string Title,
    TechniqueCategory Category,
    string? CoachingNotes,
    string? ReferenceUrl,
    int TargetWeekNumber,
    int SortOrder);

public record CurriculumThemeDto(
    Guid Id,
    string Name,
    string Category,
    string DefaultObjective,
    IReadOnlyList<ThemeDrillTemplateDto> DefaultDrills);

public sealed class GetCurriculumThemesEndpoint : EndpointWithoutRequest<IReadOnlyList<CurriculumThemeDto>>
{
    private readonly ICurriculumThemeRepository _themeRepo;

    public GetCurriculumThemesEndpoint(ICurriculumThemeRepository themeRepo)
    {
        _themeRepo = themeRepo;
    }

    public override void Configure()
    {
        Get("/api/curriculum/themes");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var themes = await _themeRepo.GetAllThemesAsync(ct);
        var dtos = themes.Select(t => new CurriculumThemeDto(
            t.Id,
            t.Name,
            t.Category,
            t.DefaultObjective,
            t.DefaultDrills.OrderBy(d => d.TargetWeekNumber).ThenBy(d => d.SortOrder).Select(d => new ThemeDrillTemplateDto(
                d.Id,
                d.Title,
                d.Category,
                d.CoachingNotes,
                d.ReferenceUrl,
                d.TargetWeekNumber,
                d.SortOrder
            )).ToList()
        )).ToList();

        await Send.OkAsync(dtos, ct);
    }
}
