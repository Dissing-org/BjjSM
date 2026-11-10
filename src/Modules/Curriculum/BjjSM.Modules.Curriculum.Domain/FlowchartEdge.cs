using BjjSM.SharedKernel;

namespace BjjSM.Modules.Curriculum.Domain;

public sealed class FlowchartEdge
{
    public Guid Id { get; private set; }
    public Guid FlowchartId { get; private set; }
    public Guid SourceNodeId { get; private set; }
    public Guid TargetNodeId { get; private set; }
    public string Label { get; private set; }
    public TransitionType Type { get; private set; }
    public string? Notes { get; private set; }
    public double CurveOffset { get; private set; }

    private FlowchartEdge()
    {
        Label = string.Empty;
    }

    public FlowchartEdge(
        Guid id,
        Guid flowchartId,
        Guid sourceNodeId,
        Guid targetNodeId,
        string label,
        TransitionType type,
        string? notes = null,
        double curveOffset = 0.0)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Edge Id cannot be empty.", nameof(id));
        if (flowchartId == Guid.Empty)
            throw new ArgumentException("FlowchartId cannot be empty.", nameof(flowchartId));
        if (sourceNodeId == Guid.Empty)
            throw new ArgumentException("Source node ID cannot be empty.", nameof(sourceNodeId));
        if (targetNodeId == Guid.Empty)
            throw new ArgumentException("Target node ID cannot be empty.", nameof(targetNodeId));
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Edge label cannot be empty.", nameof(label));

        Id = id;
        FlowchartId = flowchartId;
        SourceNodeId = sourceNodeId;
        TargetNodeId = targetNodeId;
        Label = label.Trim();
        Type = type;
        Notes = notes?.Trim();
        CurveOffset = curveOffset;
    }

    public Result UpdateDetails(string label, TransitionType type, string? notes, double curveOffset)
    {
        if (string.IsNullOrWhiteSpace(label))
            return Result.Failure("Transition label cannot be empty.");

        Label = label.Trim();
        Type = type;
        Notes = notes?.Trim();
        CurveOffset = curveOffset;
        return Result.Success();
    }
}
