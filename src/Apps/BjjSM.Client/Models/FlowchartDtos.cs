namespace BjjSM.Client.Models;

public record FlowchartPresetSummaryDto(string Key, string Title, string Description);

public record FlowchartSummaryDto(
    Guid Id,
    Guid AcademyId,
    string Title,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int NodeCount,
    int EdgeCount);

public record FlowchartNodeDto
{
    public Guid Id { get; set; }
    public Guid FlowchartId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Category { get; set; }
    public string? Notes { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; } = 160;
    public double Height { get; set; } = 80;

    public FlowchartNodeDto() { }

    public FlowchartNodeDto(Guid id, Guid flowchartId, string title, int category, string? notes, double x, double y, double width = 160, double height = 80)
    {
        Id = id;
        FlowchartId = flowchartId;
        Title = title;
        Category = category;
        Notes = notes;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}

public record FlowchartEdgeDto
{
    public Guid Id { get; set; }
    public Guid FlowchartId { get; set; }
    public Guid SourceNodeId { get; set; }
    public Guid TargetNodeId { get; set; }
    public string Label { get; set; } = string.Empty;
    public int Type { get; set; }
    public string? Notes { get; set; }
    public double CurveOffset { get; set; }

    public FlowchartEdgeDto() { }

    public FlowchartEdgeDto(Guid id, Guid flowchartId, Guid sourceNodeId, Guid targetNodeId, string label, int type, string? notes, double curveOffset)
    {
        Id = id;
        FlowchartId = flowchartId;
        SourceNodeId = sourceNodeId;
        TargetNodeId = targetNodeId;
        Label = label;
        Type = type;
        Notes = notes;
        CurveOffset = curveOffset;
    }
}

public record FlowchartDetailDto(
    Guid Id,
    Guid AcademyId,
    string Title,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<FlowchartNodeDto> Nodes,
    IReadOnlyList<FlowchartEdgeDto> Edges);

public record CreateFlowchartRequest(
    Guid AcademyId,
    string? Title,
    string? Description,
    string? PresetKey);

public record UpdateFlowchartRequest(
    string Title,
    string? Description,
    IReadOnlyList<FlowchartNodeDto> Nodes,
    IReadOnlyList<FlowchartEdgeDto> Edges);
