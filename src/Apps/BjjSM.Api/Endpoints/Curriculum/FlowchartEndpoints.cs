using BjjSM.Modules.Curriculum.Application;
using BjjSM.Modules.Curriculum.Domain;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace BjjSM.Api.Endpoints.Curriculum;

public sealed record FlowchartPresetSummaryDto(string Key, string Title, string Description);

public sealed record FlowchartSummaryDto(
    Guid Id,
    Guid AcademyId,
    string Title,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int NodeCount,
    int EdgeCount);

public sealed record FlowchartNodeDto(
    Guid Id,
    Guid FlowchartId,
    string Title,
    int Category,
    string? Notes,
    double X,
    double Y,
    double Width,
    double Height);

public sealed record FlowchartEdgeDto(
    Guid Id,
    Guid FlowchartId,
    Guid SourceNodeId,
    Guid TargetNodeId,
    string Label,
    int Type,
    string? Notes,
    double CurveOffset);

public sealed record FlowchartDetailDto(
    Guid Id,
    Guid AcademyId,
    string Title,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<FlowchartNodeDto> Nodes,
    IReadOnlyList<FlowchartEdgeDto> Edges);

public sealed record ListFlowchartsRequest
{
    [QueryParam]
    public Guid? AcademyId { get; set; }
}

public sealed record GetFlowchartByIdRequest
{
    public Guid Id { get; set; }
}

public sealed record CreateFlowchartApiRequest(
    Guid? AcademyId,
    string? Title,
    string? Description,
    string? PresetKey);

public sealed record UpdateFlowchartApiRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<FlowchartNodeDto> Nodes { get; set; } = [];
    public List<FlowchartEdgeDto> Edges { get; set; } = [];
}

public sealed record DeleteFlowchartRequest
{
    public Guid Id { get; set; }
}

public sealed class GetFlowchartPresetsEndpoint : EndpointWithoutRequest<IReadOnlyList<FlowchartPresetSummaryDto>>
{
    public override void Configure()
    {
        Get("/api/flowcharts/presets");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        var summaries = FlowchartPresets.GetPresetSummaries()
            .Select(p => new FlowchartPresetSummaryDto(p.Key, p.Title, p.Description))
            .ToList();

        return Send.OkAsync(summaries, ct);
    }
}

public sealed class ListFlowchartsEndpoint : Endpoint<ListFlowchartsRequest, IReadOnlyList<FlowchartSummaryDto>>
{
    private readonly IFlowchartRepository _repository;

    public ListFlowchartsEndpoint(IFlowchartRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Get("/api/flowcharts");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ListFlowchartsRequest req, CancellationToken ct)
    {
        var academyId = req.AcademyId ?? Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var list = await _repository.GetAllAsync(academyId, ct);
        var dtos = list.Select(f => new FlowchartSummaryDto(
            f.Id,
            f.AcademyId,
            f.Title,
            f.Description,
            f.CreatedAt,
            f.UpdatedAt,
            f.Nodes.Count,
            f.Edges.Count))
            .ToList();

        await Send.OkAsync(dtos, ct);
    }
}

public sealed class GetFlowchartByIdEndpoint : Endpoint<GetFlowchartByIdRequest, FlowchartDetailDto>
{
    private readonly IFlowchartRepository _repository;

    public GetFlowchartByIdEndpoint(IFlowchartRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Get("/api/flowcharts/{Id:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetFlowchartByIdRequest req, CancellationToken ct)
    {
        var f = await _repository.GetByIdAsync(req.Id, ct);
        if (f is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(MapToDetail(f), ct);
    }

    internal static FlowchartDetailDto MapToDetail(Flowchart f) => new(
        f.Id,
        f.AcademyId,
        f.Title,
        f.Description,
        f.CreatedAt,
        f.UpdatedAt,
        f.Nodes.Select(n => new FlowchartNodeDto(n.Id, n.FlowchartId, n.Title, (int)n.Category, n.Notes, n.X, n.Y, n.Width, n.Height)).ToList(),
        f.Edges.Select(e => new FlowchartEdgeDto(e.Id, e.FlowchartId, e.SourceNodeId, e.TargetNodeId, e.Label, (int)e.Type, e.Notes, e.CurveOffset)).ToList()
    );
}

public sealed class CreateFlowchartEndpoint : Endpoint<CreateFlowchartApiRequest, FlowchartDetailDto>
{
    private readonly IFlowchartRepository _repository;

    public CreateFlowchartEndpoint(IFlowchartRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Post("/api/flowcharts");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateFlowchartApiRequest req, CancellationToken ct)
    {
        var academyId = req.AcademyId.HasValue && req.AcademyId.Value != Guid.Empty
            ? req.AcademyId.Value
            : Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        Flowchart fc;
        if (!string.IsNullOrWhiteSpace(req.PresetKey))
        {
            var preset = FlowchartPresets.CreatePreset(req.PresetKey, academyId);
            if (preset is null)
            {
                AddError($"Preset key '{req.PresetKey}' is invalid.");
                await Send.ErrorsAsync(400, ct);
                return;
            }
            fc = preset;
            if (!string.IsNullOrWhiteSpace(req.Title))
            {
                fc.SetMetadata(req.Title, req.Description ?? fc.Description);
            }
        }
        else
        {
            var result = Flowchart.Create(academyId, req.Title ?? "New Game Plan", req.Description);
            if (result.IsFailure)
            {
                AddError(result.Error);
                await Send.ErrorsAsync(400, ct);
                return;
            }
            fc = result.Value;
        }

        await _repository.AddAsync(fc, ct);
        await Send.CreatedAtAsync($"/api/flowcharts/{fc.Id}", GetFlowchartByIdEndpoint.MapToDetail(fc), cancellation: ct);
    }
}

public sealed class UpdateFlowchartEndpoint : Endpoint<UpdateFlowchartApiRequest, FlowchartDetailDto>
{
    private readonly IFlowchartRepository _repository;

    public UpdateFlowchartEndpoint(IFlowchartRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Put("/api/flowcharts/{Id:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateFlowchartApiRequest req, CancellationToken ct)
    {
        var existing = await _repository.GetByIdAsync(req.Id, ct);
        if (existing is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var metaResult = existing.SetMetadata(req.Title, req.Description);
        if (metaResult.IsFailure)
        {
            AddError(metaResult.Error);
            await Send.ErrorsAsync(400, ct);
            return;
        }

        // Reconcile nodes
        var incomingNodeIds = (req.Nodes ?? []).Select(n => n.Id).ToHashSet();
        var currentNodes = existing.Nodes.ToList();
        foreach (var n in currentNodes)
        {
            if (!incomingNodeIds.Contains(n.Id))
            {
                existing.RemoveNode(n.Id);
            }
        }

        foreach (var nodeDto in req.Nodes ?? [])
        {
            var current = existing.Nodes.FirstOrDefault(n => n.Id == nodeDto.Id);
            if (current is null)
            {
                existing.AddNode(
                    nodeDto.Title,
                    (PositionCategory)nodeDto.Category,
                    nodeDto.Notes,
                    nodeDto.X,
                    nodeDto.Y,
                    nodeDto.Width > 0 ? nodeDto.Width : 160,
                    nodeDto.Height > 0 ? nodeDto.Height : 80,
                    nodeDto.Id != Guid.Empty ? nodeDto.Id : Guid.NewGuid());
            }
            else
            {
                existing.UpdateNodePosition(nodeDto.Id, nodeDto.X, nodeDto.Y);
                existing.UpdateNodeDetails(
                    nodeDto.Id,
                    nodeDto.Title,
                    (PositionCategory)nodeDto.Category,
                    nodeDto.Notes,
                    nodeDto.Width > 0 ? nodeDto.Width : 160,
                    nodeDto.Height > 0 ? nodeDto.Height : 80);
            }
        }

        // Reconcile edges
        var incomingEdgeIds = (req.Edges ?? []).Select(e => e.Id).ToHashSet();
        var currentEdges = existing.Edges.ToList();
        foreach (var e in currentEdges)
        {
            if (!incomingEdgeIds.Contains(e.Id))
            {
                existing.RemoveEdge(e.Id);
            }
        }

        foreach (var edgeDto in req.Edges ?? [])
        {
            var current = existing.Edges.FirstOrDefault(e => e.Id == edgeDto.Id);
            if (current is null)
            {
                existing.AddEdge(
                    edgeDto.SourceNodeId,
                    edgeDto.TargetNodeId,
                    edgeDto.Label,
                    (TransitionType)edgeDto.Type,
                    edgeDto.Notes,
                    edgeDto.CurveOffset,
                    edgeDto.Id != Guid.Empty ? edgeDto.Id : Guid.NewGuid());
            }
            else
            {
                existing.UpdateEdgeDetails(
                    edgeDto.Id,
                    edgeDto.Label,
                    (TransitionType)edgeDto.Type,
                    edgeDto.Notes,
                    edgeDto.CurveOffset);
            }
        }

        await _repository.UpdateAsync(existing, ct);
        await Send.OkAsync(GetFlowchartByIdEndpoint.MapToDetail(existing), ct);
    }
}

public sealed class DeleteFlowchartEndpoint : Endpoint<DeleteFlowchartRequest>
{
    private readonly IFlowchartRepository _repository;

    public DeleteFlowchartEndpoint(IFlowchartRepository repository)
    {
        _repository = repository;
    }

    public override void Configure()
    {
        Delete("/api/flowcharts/{Id:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteFlowchartRequest req, CancellationToken ct)
    {
        var existing = await _repository.GetByIdAsync(req.Id, ct);
        if (existing is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await _repository.DeleteAsync(req.Id, ct);
        await Send.NoContentAsync(ct);
    }
}
