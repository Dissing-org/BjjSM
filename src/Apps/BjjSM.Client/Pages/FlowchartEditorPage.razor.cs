using System.Globalization;
using System.Text;
using System.Text.Json;
using BjjSM.Client.Models;
using BjjSM.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BjjSM.Client.Pages;

public partial class FlowchartEditorPage : ComponentBase, IAsyncDisposable
{
    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    public IFlowchartApiClient FlowchartApiClient { get; set; } = default!;

    [Inject]
    public IJSRuntime JS { get; set; } = default!;

    private IJSObjectReference? _interopModule;

    protected bool _isLoading = true;
    protected string? _errorMessage;
    protected string? _notificationMessage;
    protected bool _isSaving;
    protected bool _isDirty;

    protected FlowchartDetailDto? _flowchart;
    protected string _title = string.Empty;
    protected string? _description;

    protected List<FlowchartNodeDto> _nodes = [];
    protected List<FlowchartEdgeDto> _edges = [];

    // Canvas view state
    protected double _zoom = 1.0;
    protected double _panX = 0;
    protected double _panY = 0;
    protected bool _isPanning;
    protected double _dragStartX;
    protected double _dragStartY;

    // Pointer & Multi-touch state
    protected readonly Dictionary<long, (double X, double Y)> _activePointers = new();
    protected double _initialPinchDistance = 0;
    protected double _initialPinchZoom = 1.0;

    // Drag slop threshold state
    protected const double DragThreshold = 6.0;
    protected double _downClientX;
    protected double _downClientY;
    protected bool _hasDragged;

    // Node drag state
    protected FlowchartNodeDto? _draggingNode;
    protected double _nodeDragOffsetX;
    protected double _nodeDragOffsetY;

    // Selection & connection state
    protected FlowchartNodeDto? _selectedNode;
    protected FlowchartEdgeDto? _selectedEdge;
    protected bool _isConnectingMode;
    protected Guid? _selectedSourceNodeId;

    protected override async Task OnInitializedAsync()
    {
        await LoadFlowchartAsync();
    }

    protected async Task LoadFlowchartAsync()
    {
        _isLoading = true;
        _errorMessage = null;

        var result = await FlowchartApiClient.GetFlowchartByIdAsync(Id);
        if (result.IsSuccess)
        {
            _flowchart = result.Value;
            _title = _flowchart.Title;
            _description = _flowchart.Description;
            _nodes = _flowchart.Nodes.Select(n => new FlowchartNodeDto(n.Id, n.FlowchartId, n.Title, n.Category, n.Notes, n.X, n.Y, n.Width, n.Height)).ToList();
            _edges = _flowchart.Edges.Select(e => new FlowchartEdgeDto(e.Id, e.FlowchartId, e.SourceNodeId, e.TargetNodeId, e.Label, e.Type, e.Notes, e.CurveOffset)).ToList();
            _isDirty = false;
        }
        else
        {
            _errorMessage = result.Error;
        }

        _isLoading = false;
    }

    protected void MarkDirty()
    {
        _isDirty = true;
    }

    protected void ToggleConnectMode()
    {
        _isConnectingMode = !_isConnectingMode;
        _selectedSourceNodeId = null;
        if (_isConnectingMode)
        {
            _selectedNode = null;
            _selectedEdge = null;
        }
    }

    protected void HandleNodeClick(FlowchartNodeDto node)
    {
        if (_hasDragged)
        {
            _hasDragged = false;
            return;
        }

        if (_isConnectingMode)
        {
            if (!_selectedSourceNodeId.HasValue)
            {
                _selectedSourceNodeId = node.Id;
            }
            else
            {
                var sourceId = _selectedSourceNodeId.Value;
                var targetId = node.Id;

                var existingParallelEdges = _edges.Count(e =>
                    (e.SourceNodeId == sourceId && e.TargetNodeId == targetId) ||
                    (e.SourceNodeId == targetId && e.TargetNodeId == sourceId));

                var curveOffset = existingParallelEdges == 0 ? 0 : (existingParallelEdges % 2 == 1 ? 30 * ((existingParallelEdges + 1) / 2) : -30 * (existingParallelEdges / 2));

                var newEdge = new FlowchartEdgeDto(
                    Guid.NewGuid(),
                    Id,
                    sourceId,
                    targetId,
                    "Ny overgang",
                    6, // Transition
                    null,
                    curveOffset);

                _edges.Add(newEdge);
                _selectedEdge = newEdge;
                _selectedNode = null;
                _selectedSourceNodeId = null;
                _isConnectingMode = false;
                MarkDirty();
            }
        }
        else
        {
            _selectedNode = node;
            _selectedEdge = null;
        }
    }

    protected void HandleNodeKeyDown(FlowchartNodeDto node, KeyboardEventArgs e)
    {
        if (e.Key is "Enter" or " " or "Spacebar")
        {
            HandleNodeClick(node);
        }
    }

    protected void SelectEdge(FlowchartEdgeDto edge)
    {
        if (_hasDragged)
        {
            return;
        }

        if (!_isConnectingMode)
        {
            _selectedEdge = edge;
            _selectedNode = null;
        }
    }

    protected void HandleEdgeKeyDown(FlowchartEdgeDto edge, KeyboardEventArgs e)
    {
        if (e.Key is "Enter" or " " or "Spacebar")
        {
            SelectEdge(edge);
        }
    }

    protected void AddPositionNode()
    {
        var centerX = (-_panX + 300) / _zoom;
        var centerY = (-_panY + 200) / _zoom;

        var newNode = new FlowchartNodeDto(
            Guid.NewGuid(),
            Id,
            "Ny position",
            1, // Top
            null,
            Math.Round(centerX, 0),
            Math.Round(centerY, 0),
            160,
            80);

        _nodes.Add(newNode);
        _selectedNode = newNode;
        _selectedEdge = null;
        MarkDirty();
    }

    protected void DeleteNode(Guid nodeId)
    {
        _nodes.RemoveAll(n => n.Id == nodeId);
        _edges.RemoveAll(e => e.SourceNodeId == nodeId || e.TargetNodeId == nodeId);
        if (_selectedNode?.Id == nodeId)
        {
            _selectedNode = null;
        }
        MarkDirty();
    }

    protected void DeleteEdge(Guid edgeId)
    {
        _edges.RemoveAll(e => e.Id == edgeId);
        if (_selectedEdge?.Id == edgeId)
        {
            _selectedEdge = null;
        }
        MarkDirty();
    }

    // Canvas Pointer & Wheel Handlers
    protected void OnCanvasPointerDown(PointerEventArgs e)
    {
        _activePointers[e.PointerId] = (e.ClientX, e.ClientY);
        _hasDragged = false;
        _downClientX = e.ClientX;
        _downClientY = e.ClientY;

        if (_activePointers.Count == 1 && e.Button == 0 && _draggingNode is null)
        {
            _isPanning = true;
            _dragStartX = e.ClientX - _panX;
            _dragStartY = e.ClientY - _panY;
            _selectedNode = null;
            _selectedEdge = null;
        }
        else if (_activePointers.Count == 2)
        {
            var pts = _activePointers.Values.ToArray();
            var dx = pts[0].X - pts[1].X;
            var dy = pts[0].Y - pts[1].Y;
            _initialPinchDistance = Math.Sqrt(dx * dx + dy * dy);
            _initialPinchZoom = _zoom;
            _isPanning = false;
        }
    }

    protected void OnCanvasPointerMove(PointerEventArgs e)
    {
        if (!_activePointers.ContainsKey(e.PointerId))
        {
            return;
        }

        _activePointers[e.PointerId] = (e.ClientX, e.ClientY);

        // Multi-touch pinch zoom
        if (_activePointers.Count >= 2)
        {
            var pts = _activePointers.Values.Take(2).ToArray();
            var dx = pts[0].X - pts[1].X;
            var dy = pts[0].Y - pts[1].Y;
            var currentDistance = Math.Sqrt(dx * dx + dy * dy);

            if (_initialPinchDistance <= 0)
            {
                _initialPinchDistance = currentDistance;
                _initialPinchZoom = _zoom;
            }
            else if (currentDistance > 0 && _initialPinchDistance > 0)
            {
                _zoom = Math.Clamp(_initialPinchZoom * (currentDistance / _initialPinchDistance), 0.2, 3.0);
            }

            _hasDragged = true;
            return;
        }

        // Single pointer tracking
        var moveDist = Math.Sqrt(Math.Pow(e.ClientX - _downClientX, 2) + Math.Pow(e.ClientY - _downClientY, 2));
        if (moveDist > DragThreshold)
        {
            _hasDragged = true;
        }

        if (_draggingNode is not null)
        {
            var newX = (e.ClientX - _panX) / _zoom - _nodeDragOffsetX;
            var newY = (e.ClientY - _panY) / _zoom - _nodeDragOffsetY;

            var index = _nodes.FindIndex(n => n.Id == _draggingNode.Id);
            if (index >= 0)
            {
                var updated = _nodes[index] with { X = Math.Round(newX, 0), Y = Math.Round(newY, 0) };
                _nodes[index] = updated;
                _draggingNode = updated;
                if (_selectedNode?.Id == updated.Id)
                {
                    _selectedNode = updated;
                }
            }
        }
        else if (_isPanning)
        {
            _panX = e.ClientX - _dragStartX;
            _panY = e.ClientY - _dragStartY;
        }
    }

    protected void OnCanvasPointerUp(PointerEventArgs e)
    {
        _activePointers.Remove(e.PointerId);

        if (_hasDragged && _draggingNode is not null)
        {
            MarkDirty();
        }

        if (_activePointers.Count < 2)
        {
            _initialPinchDistance = 0;
        }

        if (_activePointers.Count == 0)
        {
            _isPanning = false;
            _draggingNode = null;
            _initialPinchDistance = 0;
        }
    }

    protected void OnCanvasPointerCancel(PointerEventArgs e)
    {
        OnCanvasPointerUp(e);
    }

    protected void OnCanvasWheel(WheelEventArgs e)
    {
        var factor = e.DeltaY < 0 ? 1.1 : 0.9;
        _zoom = Math.Clamp(_zoom * factor, 0.2, 3.0);
    }

    protected void OnNodePointerDown(FlowchartNodeDto node, PointerEventArgs e)
    {
        _activePointers[e.PointerId] = (e.ClientX, e.ClientY);
        _hasDragged = false;
        _downClientX = e.ClientX;
        _downClientY = e.ClientY;

        if (!_isConnectingMode && e.Button == 0)
        {
            _draggingNode = node;
            var canvasMouseX = (e.ClientX - _panX) / _zoom;
            var canvasMouseY = (e.ClientY - _panY) / _zoom;
            _nodeDragOffsetX = canvasMouseX - node.X;
            _nodeDragOffsetY = canvasMouseY - node.Y;
            _selectedNode = node;
            _selectedEdge = null;
        }
    }

    protected void ZoomIn()
    {
        _zoom = Math.Clamp(_zoom * 1.2, 0.2, 3.0);
    }

    protected void ZoomOut()
    {
        _zoom = Math.Clamp(_zoom * 0.8, 0.2, 3.0);
    }

    protected void ResetView()
    {
        _zoom = 1.0;
        _panX = 0;
        _panY = 0;
    }

    protected async Task SaveFlowchartAsync()
    {
        _isSaving = true;
        _notificationMessage = null;

        var req = new UpdateFlowchartRequest(_title, _description, _nodes, _edges);
        var res = await FlowchartApiClient.UpdateFlowchartAsync(Id, req);

        if (res.IsSuccess)
        {
            _flowchart = res.Value;
            _isDirty = false;
            _notificationMessage = "Flowchart blev gemt!";
        }
        else
        {
            _errorMessage = $"Gem fejlede: {res.Error}";
        }

        _isSaving = false;
    }

    protected async Task ExportJsonAsync()
    {
        var req = new UpdateFlowchartRequest(_title, _description, _nodes, _edges);
        var json = JsonSerializer.Serialize(req, new JsonSerializerOptions { WriteIndented = true });
        var bytes = Encoding.UTF8.GetBytes(json);

        var filename = $"{SanitizeFileName(_title)}.json";
        await DownloadFileAsync(filename, bytes, "application/json");
        _notificationMessage = $"Eksporterede '{filename}'";
    }

    protected async Task ImportJsonAsync(InputFileChangeEventArgs e)
    {
        try
        {
            using var stream = e.File.OpenReadStream(1024 * 1024);
            var req = await JsonSerializer.DeserializeAsync<UpdateFlowchartRequest>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (req is null)
            {
                _errorMessage = "Kunne ikke læse JSON-fil: tomt indhold.";
                return;
            }

            if (!ValidateImportedRequest(req, out var validationError))
            {
                _errorMessage = validationError;
                return;
            }

            _title = req.Title;
            _description = req.Description;
            _nodes = req.Nodes.ToList();
            _edges = req.Edges.ToList();
            _selectedNode = null;
            _selectedEdge = null;
            _errorMessage = null;
            MarkDirty();
            _notificationMessage = "Flowchart-JSON blev importeret!";
        }
        catch (Exception ex)
        {
            _errorMessage = $"Kunne ikke læse JSON-fil: {ex.Message}";
        }
    }

    protected static bool ValidateImportedRequest(UpdateFlowchartRequest req, out string? error)
    {
        if (req.Nodes is null || req.Edges is null)
        {
            error = "Ugyldig flowchart-JSON: både positioner og pile er påkrævet.";
            return false;
        }

        foreach (var node in req.Nodes)
        {
            if (!double.IsFinite(node.X)
                || !double.IsFinite(node.Y)
                || !double.IsFinite(node.Width)
                || !double.IsFinite(node.Height)
                || node.Width <= 0
                || node.Height <= 0)
            {
                error = "Ugyldig flowchart-JSON: positioners koordinater og dimensioner skal være gyldige tal med positiv bredde og højde.";
                return false;
            }
        }

        error = null;
        return true;
    }

    protected string BuildSvgMarkup()
    {
        var edgeLayouts = FlowchartLayoutEngine.ComputeEdgeLayouts(_nodes, _edges);
        var svgBuilder = new StringBuilder();
        svgBuilder.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1200\" height=\"800\" viewBox=\"0 0 1200 800\" style=\"background-color: #F8FAFC; font-family: system-ui, -apple-system, sans-serif;\">");
        svgBuilder.AppendLine("  <defs>");
        svgBuilder.AppendLine("    <marker id=\"arrowhead\" markerWidth=\"10\" markerHeight=\"7\" refX=\"10\" refY=\"3.5\" orient=\"auto\">");
        svgBuilder.AppendLine("      <polygon points=\"0 0, 10 3.5, 0 7\" fill=\"#20409A\" />");
        svgBuilder.AppendLine("    </marker>");
        svgBuilder.AppendLine("    <marker id=\"arrowhead-gold\" markerWidth=\"10\" markerHeight=\"7\" refX=\"10\" refY=\"3.5\" orient=\"auto\">");
        svgBuilder.AppendLine("      <polygon points=\"0 0, 10 3.5, 0 7\" fill=\"#B38F00\" />");
        svgBuilder.AppendLine("    </marker>");
        svgBuilder.AppendLine("  </defs>");

        // Layer 1: Arrow paths
        foreach (var edge in _edges)
        {
            if (!edgeLayouts.TryGetValue(edge.Id, out var layout) || string.IsNullOrEmpty(layout.PathData))
            {
                continue;
            }

            var isSub = edge.Type == 3;
            var stroke = isSub ? "#B38F00" : "#20409A";
            var marker = isSub ? "url(#arrowhead-gold)" : "url(#arrowhead)";
            var dashAttr = edge.Type == 4 ? " stroke-dasharray=\"4 4\"" : string.Empty;

            svgBuilder.AppendLine($"  <path d=\"{layout.PathData}\" fill=\"none\" stroke=\"{stroke}\" stroke-width=\"2.5\"{dashAttr} marker-end=\"{marker}\" />");
        }

        // Layer 2: Position nodes
        foreach (var node in _nodes)
        {
            var isSub = node.Category == 4;
            var headerColor = isSub ? "#FFEE00" : "#20409A";
            var headerTextColor = isSub ? "#050505" : "#FFFFFF";
            var borderColor = isSub ? "#FFD700" : "#20409A";

            svgBuilder.AppendLine(FormattableString.Invariant($"  <g transform=\"translate({node.X}, {node.Y})\">"));
            svgBuilder.AppendLine(FormattableString.Invariant($"    <rect width=\"{node.Width}\" height=\"{node.Height}\" rx=\"8\" fill=\"#FFFFFF\" stroke=\"{borderColor}\" stroke-width=\"2\" />"));
            svgBuilder.AppendLine(FormattableString.Invariant($"    <path d=\"M 0 8 Q 0 0 8 0 L {node.Width - 8} 0 Q {node.Width} 0 {node.Width} 8 L {node.Width} 26 L 0 26 Z\" fill=\"{headerColor}\" />"));
            svgBuilder.AppendLine($"    <text x=\"8\" y=\"18\" font-size=\"10\" font-weight=\"bold\" fill=\"{headerTextColor}\">{GetCategoryDisplayName(node.Category).ToUpperInvariant()}</text>");
            svgBuilder.AppendLine($"    <text x=\"10\" y=\"48\" font-size=\"13\" font-weight=\"bold\" fill=\"#050505\">{System.Net.WebUtility.HtmlEncode(node.Title)}</text>");
            if (!string.IsNullOrWhiteSpace(node.Notes))
            {
                svgBuilder.AppendLine($"    <text x=\"10\" y=\"66\" font-size=\"10\" fill=\"#64748B\">{System.Net.WebUtility.HtmlEncode(node.Notes)}</text>");
            }
            svgBuilder.AppendLine("  </g>");
        }

        // Layer 3: Arrow label pills & leader lines (above nodes)
        foreach (var edge in _edges)
        {
            if (!edgeLayouts.TryGetValue(edge.Id, out var layout) || string.IsNullOrEmpty(layout.PathData))
            {
                continue;
            }

            var isSub = edge.Type == 3;
            var stroke = isSub ? "#B38F00" : "#20409A";
            var pillFill = isSub ? "#FFFBEB" : "#FFFFFF";
            var textFill = isSub ? "#997A00" : "#050505";

            if (layout.HasLeaderLine)
            {
                svgBuilder.AppendLine(FormattableString.Invariant(
                    $"  <line x1=\"{layout.AnchorX:F1}\" y1=\"{layout.AnchorY:F1}\" x2=\"{layout.LabelX:F1}\" y2=\"{layout.LabelY:F1}\" stroke=\"{stroke}\" stroke-width=\"1\" stroke-dasharray=\"3 3\" opacity=\"0.65\" />"));
            }

            svgBuilder.AppendLine(FormattableString.Invariant(
                $"  <rect x=\"{layout.LabelX - layout.PillWidth / 2}\" y=\"{layout.LabelY - layout.PillHeight / 2}\" width=\"{layout.PillWidth}\" height=\"{layout.PillHeight}\" rx=\"12\" fill=\"{pillFill}\" stroke=\"{stroke}\" stroke-width=\"1.5\" />"));
            svgBuilder.AppendLine(FormattableString.Invariant(
                $"  <text x=\"{layout.LabelX}\" y=\"{layout.LabelY + 4}\" text-anchor=\"middle\" font-size=\"11\" font-weight=\"bold\" fill=\"{textFill}\">{System.Net.WebUtility.HtmlEncode(edge.Label)}</text>"));
        }

        svgBuilder.AppendLine("</svg>");
        return svgBuilder.ToString();
    }

    protected async Task ExportSvgAsync()
    {
        var bytes = Encoding.UTF8.GetBytes(BuildSvgMarkup());
        var filename = $"{SanitizeFileName(_title)}.svg";
        await DownloadFileAsync(filename, bytes, "image/svg+xml");
        _notificationMessage = $"Downloadede '{filename}'";
    }

    private async Task DownloadFileAsync(string fileName, byte[] bytes, string contentType)
    {
        using var stream = new MemoryStream(bytes);
        using var streamRef = new DotNetStreamReference(stream);
        _interopModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "./js/app-interop.js");
        await _interopModule.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef, contentType);
    }

    public async ValueTask DisposeAsync()
    {
        if (_interopModule is not null)
        {
            try
            {
                await _interopModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            _interopModule = null;
        }
    }

    // Geometry Calculation
    protected (string PathData, double LabelX, double LabelY) CalculateEdgeGeometry(FlowchartEdgeDto edge)
    {
        var edgesForLayout = _edges.Any(e => e.Id == edge.Id)
            ? _edges
            : [.. _edges, edge];

        var layouts = FlowchartLayoutEngine.ComputeEdgeLayouts(_nodes, edgesForLayout);
        return layouts.TryGetValue(edge.Id, out var layout)
            ? (layout.PathData, layout.LabelX, layout.LabelY)
            : (string.Empty, 0, 0);
    }

    protected static string GetCategoryDisplayName(int category) => category switch
    {
        1 => "Topkontrol",
        2 => "Bundguard",
        3 => "Neutral",
        4 => "Submission",
        _ => "Position"
    };

    protected static string TruncateText(string text, int max)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        return text.Length <= max ? text : text[..max] + "...";
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var clean = new string(name.Where(ch => !invalid.Contains(ch)).ToArray());
        return string.IsNullOrWhiteSpace(clean) ? "flowchart" : clean.Trim().Replace(' ', '-').ToLowerInvariant();
    }
}
