using BjjSM.SharedKernel;

namespace BjjSM.Modules.Curriculum.Domain;

public sealed class FlowchartNode
{
    public Guid Id { get; private set; }
    public Guid FlowchartId { get; private set; }
    public string Title { get; private set; }
    public PositionCategory Category { get; private set; }
    public string? Notes { get; private set; }
    public double X { get; private set; }
    public double Y { get; private set; }
    public double Width { get; private set; }
    public double Height { get; private set; }

    private FlowchartNode()
    {
        Title = string.Empty;
    }

    public FlowchartNode(
        Guid id,
        Guid flowchartId,
        string title,
        PositionCategory category,
        string? notes,
        double x,
        double y,
        double width = 160,
        double height = 80)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Node Id cannot be empty.", nameof(id));
        if (flowchartId == Guid.Empty)
            throw new ArgumentException("FlowchartId cannot be empty.", nameof(flowchartId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Node title cannot be empty.", nameof(title));
        if (width <= 0)
            throw new ArgumentException("Node width must be greater than zero.", nameof(width));
        if (height <= 0)
            throw new ArgumentException("Node height must be greater than zero.", nameof(height));

        Id = id;
        FlowchartId = flowchartId;
        Title = title.Trim();
        Category = category;
        Notes = notes?.Trim();
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public Result UpdatePosition(double x, double y)
    {
        X = x;
        Y = y;
        return Result.Success();
    }

    public Result UpdateDetails(string title, PositionCategory category, string? notes, double width = 160, double height = 80)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure("Node title cannot be empty.");
        if (width <= 0)
            return Result.Failure("Node width must be greater than zero.");
        if (height <= 0)
            return Result.Failure("Node height must be greater than zero.");

        Title = title.Trim();
        Category = category;
        Notes = notes?.Trim();
        Width = width;
        Height = height;
        return Result.Success();
    }
}
