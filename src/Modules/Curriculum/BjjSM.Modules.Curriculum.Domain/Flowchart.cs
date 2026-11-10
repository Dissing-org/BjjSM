using BjjSM.SharedKernel;

namespace BjjSM.Modules.Curriculum.Domain;

public sealed class Flowchart
{
    private readonly List<FlowchartNode> _nodes = [];
    private readonly List<FlowchartEdge> _edges = [];

    public Guid Id { get; private set; }
    public Guid AcademyId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<FlowchartNode> Nodes => _nodes.AsReadOnly();
    public IReadOnlyList<FlowchartEdge> Edges => _edges.AsReadOnly();

    private Flowchart()
    {
        Title = string.Empty;
    }

    private Flowchart(Guid id, Guid academyId, string title, string? description)
    {
        Id = id;
        AcademyId = academyId;
        Title = title.Trim();
        Description = description?.Trim();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Flowchart> Create(Guid academyId, string title, string? description = null, Guid? id = null)
    {
        if (academyId == Guid.Empty)
            return Result.Failure<Flowchart>("AcademyId is required.");

        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Flowchart>("Title cannot be empty.");

        var flowchart = new Flowchart(id ?? Guid.NewGuid(), academyId, title, description);
        return Result.Success(flowchart);
    }

    public Result SetMetadata(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure("Title cannot be empty.");

        Title = title.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result<FlowchartNode> AddNode(
        string title,
        PositionCategory category,
        string? notes = null,
        double x = 0,
        double y = 0,
        double width = 160,
        double height = 80,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<FlowchartNode>("Node title cannot be empty.");

        var nodeId = id ?? Guid.NewGuid();
        if (_nodes.Any(n => n.Id == nodeId))
            return Result.Failure<FlowchartNode>($"A node with ID '{nodeId}' already exists.");

        try
        {
            var node = new FlowchartNode(nodeId, Id, title, category, notes, x, y, width, height);
            _nodes.Add(node);
            UpdatedAt = DateTime.UtcNow;
            return Result.Success(node);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<FlowchartNode>(ex.Message);
        }
    }

    public Result UpdateNodePosition(Guid nodeId, double x, double y)
    {
        var node = _nodes.FirstOrDefault(n => n.Id == nodeId);
        if (node is null)
            return Result.Failure($"Node with ID '{nodeId}' does not exist.");

        var result = node.UpdatePosition(x, y);
        if (result.IsSuccess)
        {
            UpdatedAt = DateTime.UtcNow;
        }
        return result;
    }

    public Result UpdateNodeDetails(Guid nodeId, string title, PositionCategory category, string? notes, double width = 160, double height = 80)
    {
        var node = _nodes.FirstOrDefault(n => n.Id == nodeId);
        if (node is null)
            return Result.Failure($"Node with ID '{nodeId}' does not exist.");

        var result = node.UpdateDetails(title, category, notes, width, height);
        if (result.IsSuccess)
        {
            UpdatedAt = DateTime.UtcNow;
        }
        return result;
    }

    public Result RemoveNode(Guid nodeId)
    {
        var node = _nodes.FirstOrDefault(n => n.Id == nodeId);
        if (node is null)
            return Result.Failure($"Node with ID '{nodeId}' does not exist.");

        _nodes.Remove(node);
        _edges.RemoveAll(e => e.SourceNodeId == nodeId || e.TargetNodeId == nodeId);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result<FlowchartEdge> AddEdge(
        Guid sourceNodeId,
        Guid targetNodeId,
        string label,
        TransitionType type,
        string? notes = null,
        double curveOffset = 0.0,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(label))
            return Result.Failure<FlowchartEdge>("Transition label cannot be empty.");

        if (!_nodes.Any(n => n.Id == sourceNodeId))
            return Result.Failure<FlowchartEdge>($"Source node '{sourceNodeId}' does not exist in flowchart.");

        if (!_nodes.Any(n => n.Id == targetNodeId))
            return Result.Failure<FlowchartEdge>($"Target node '{targetNodeId}' does not exist in flowchart.");

        var edgeId = id ?? Guid.NewGuid();
        if (_edges.Any(e => e.Id == edgeId))
            return Result.Failure<FlowchartEdge>($"An edge with ID '{edgeId}' already exists.");

        try
        {
            var edge = new FlowchartEdge(edgeId, Id, sourceNodeId, targetNodeId, label, type, notes, curveOffset);
            _edges.Add(edge);
            UpdatedAt = DateTime.UtcNow;
            return Result.Success(edge);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<FlowchartEdge>(ex.Message);
        }
    }

    public Result UpdateEdgeDetails(Guid edgeId, string label, TransitionType type, string? notes, double curveOffset)
    {
        var edge = _edges.FirstOrDefault(e => e.Id == edgeId);
        if (edge is null)
            return Result.Failure($"Edge with ID '{edgeId}' does not exist.");

        var result = edge.UpdateDetails(label, type, notes, curveOffset);
        if (result.IsSuccess)
        {
            UpdatedAt = DateTime.UtcNow;
        }
        return result;
    }

    public Result RemoveEdge(Guid edgeId)
    {
        var edge = _edges.FirstOrDefault(e => e.Id == edgeId);
        if (edge is null)
            return Result.Failure($"Edge with ID '{edgeId}' does not exist.");

        _edges.Remove(edge);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
