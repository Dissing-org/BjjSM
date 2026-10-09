using BjjSM.Modules.Curriculum.Application;
using FastEndpoints;

namespace BjjSM.Api.Endpoints.Curriculum;

public sealed class DownloadPlanPdfEndpoint : EndpointWithoutRequest
{
    private readonly IMonthlyPlanRepository _repository;
    private readonly ISyllabusPdfGenerator _pdfGenerator;

    public DownloadPlanPdfEndpoint(
        IMonthlyPlanRepository repository,
        ISyllabusPdfGenerator pdfGenerator)
    {
        _repository = repository;
        _pdfGenerator = pdfGenerator;
    }

    public override void Configure()
    {
        Get("/api/v1/plans/{id:guid}/pdf");
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

        var pdfBytes = _pdfGenerator.Generate(
            plan,
            academyName: "BJJ ACADEMY",
            programName: "CURRICULUM SYLLABUS",
            instructorName: "HEAD INSTRUCTOR");

        var fileName = $"Syllabus_{plan.Year}_{plan.Month:D2}.pdf";
        await Send.BytesAsync(pdfBytes, fileName, contentType: "application/pdf", cancellation: ct);
    }
}
