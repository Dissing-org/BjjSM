using System.Globalization;
using BjjSM.Client.Models;
using BjjSM.Client.Pages;
using BjjSM.Client.Services;
using BjjSM.Modules.Curriculum.Domain;
using Xunit;

namespace BjjSM.Client.Tests;

public class FlowchartLayoutEngineTests
{
    private sealed class SvgTestableFlowchartEditor : FlowchartEditorPage
    {
        public void SetNodes(List<FlowchartNodeDto> nodes) => _nodes = nodes;
        public void SetEdges(List<FlowchartEdgeDto> edges) => _edges = edges;
        public string TestBuildSvgMarkup() => BuildSvgMarkup();
    }

    private static bool BoxesOverlap(
        double leftA, double topA, double widthA, double heightA,
        double leftB, double topB, double widthB, double heightB) =>
        leftA < leftB + widthB &&
        leftA + widthA > leftB &&
        topA < topB + heightB &&
        topA + heightA > topB;

    private static bool PillsOverlap(EdgeLayout a, EdgeLayout b) =>
        BoxesOverlap(
            a.LabelX - a.PillWidth / 2.0, a.LabelY - a.PillHeight / 2.0, a.PillWidth, a.PillHeight,
            b.LabelX - b.PillWidth / 2.0, b.LabelY - b.PillHeight / 2.0, b.PillWidth, b.PillHeight);

    private static bool PillOverlapsNode(EdgeLayout layout, FlowchartNodeDto node) =>
        BoxesOverlap(
            layout.LabelX - layout.PillWidth / 2.0, layout.LabelY - layout.PillHeight / 2.0, layout.PillWidth, layout.PillHeight,
            node.X, node.Y, node.Width, node.Height);

    [Fact]
    public void ComputeEdgeLayouts_BidirectionalEdgesWithZeroOffset_ShouldSeparateOntoOppositeCurvesWithoutOverlap()
    {
        var flowchartId = Guid.NewGuid();
        var nodeA = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Armbar", 4, null, 100, 200, 160, 80);
        var nodeB = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Triangle", 4, null, 500, 200, 160, 80);

        var edgeAB = new FlowchartEdgeDto(Guid.NewGuid(), flowchartId, nodeA.Id, nodeB.Id, "Triangle Counter", 6, null, 0);
        var edgeBA = new FlowchartEdgeDto(Guid.NewGuid(), flowchartId, nodeB.Id, nodeA.Id, "Armlock Finish", 3, null, 0);

        var layouts = FlowchartLayoutEngine.ComputeEdgeLayouts([nodeA, nodeB], [edgeAB, edgeBA]);

        var layoutAB = layouts[edgeAB.Id];
        var layoutBA = layouts[edgeBA.Id];

        Assert.NotEqual(layoutAB.PathData, layoutBA.PathData);
        Assert.False(PillsOverlap(layoutAB, layoutBA), "Bidirectional edge label pills must not overlap.");
        Assert.False(PillOverlapsNode(layoutAB, nodeA));
        Assert.False(PillOverlapsNode(layoutAB, nodeB));
        Assert.False(PillOverlapsNode(layoutBA, nodeA));
        Assert.False(PillOverlapsNode(layoutBA, nodeB));

        // Verify labels sit on opposite sides of the centerline y = 240
        Assert.True(
            (layoutAB.LabelY < 240 && layoutBA.LabelY > 240) ||
            (layoutAB.LabelY > 240 && layoutBA.LabelY < 240),
            $"Expected opposite sides of y=240, got AB.Y={layoutAB.LabelY} and BA.Y={layoutBA.LabelY}");
    }

    [Fact]
    public void ComputeEdgeLayouts_ThreeParallelEdges_ShouldHaveZeroPairwiseLabelOverlaps()
    {
        var flowchartId = Guid.NewGuid();
        var nodeA = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Closed Guard", 2, null, 80, 240, 160, 80);
        var nodeB = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Mount", 1, null, 520, 240, 160, 80);

        var edges = new List<FlowchartEdgeDto>
        {
            new(Guid.NewGuid(), flowchartId, nodeA.Id, nodeB.Id, "Hip Bump Sweep", 1, null, 0),
            new(Guid.NewGuid(), flowchartId, nodeA.Id, nodeB.Id, "Flower Sweep", 1, null, 0),
            new(Guid.NewGuid(), flowchartId, nodeA.Id, nodeB.Id, "Scissor Sweep", 1, null, 0)
        };

        var layouts = FlowchartLayoutEngine.ComputeEdgeLayouts([nodeA, nodeB], edges);
        var layoutList = edges.Select(e => layouts[e.Id]).ToList();

        // All 3 paths must be distinct
        Assert.Equal(3, layoutList.Select(l => l.PathData).Distinct().Count());

        for (var i = 0; i < layoutList.Count; i++)
        {
            Assert.False(PillOverlapsNode(layoutList[i], nodeA));
            Assert.False(PillOverlapsNode(layoutList[i], nodeB));

            for (var j = i + 1; j < layoutList.Count; j++)
            {
                Assert.False(
                    PillsOverlap(layoutList[i], layoutList[j]),
                    $"Parallel edge labels {i} and {j} overlapped.");
            }
        }
    }

    [Theory]
    [InlineData(FlowchartPresets.ClosedGuardKey)]
    [InlineData(FlowchartPresets.HalfGuardKey)]
    [InlineData(FlowchartPresets.MountEscapesKey)]
    public void ComputeEdgeLayouts_AllStarterPresets_ShouldHaveZeroLabelLabelAndZeroLabelNodeOverlaps(string presetKey)
    {
        var preset = FlowchartPresets.CreatePreset(presetKey, Guid.NewGuid());
        Assert.NotNull(preset);

        var nodes = preset.Nodes
            .Select(n => new FlowchartNodeDto(n.Id, preset.Id, n.Title, (int)n.Category, n.Notes, n.X, n.Y, n.Width, n.Height))
            .ToList();

        var edges = preset.Edges
            .Select(e => new FlowchartEdgeDto(e.Id, preset.Id, e.SourceNodeId, e.TargetNodeId, e.Label, (int)e.Type, e.Notes, e.CurveOffset))
            .ToList();

        var layouts = FlowchartLayoutEngine.ComputeEdgeLayouts(nodes, edges);
        Assert.Equal(edges.Count, layouts.Count);

        var layoutList = edges.Select(e => (Edge: e, Layout: layouts[e.Id])).ToList();

        for (var i = 0; i < layoutList.Count; i++)
        {
            var (edgeA, layoutA) = layoutList[i];
            Assert.NotEmpty(layoutA.PathData);

            foreach (var node in nodes)
            {
                Assert.False(
                    PillOverlapsNode(layoutA, node),
                    $"Preset '{presetKey}': edge '{edgeA.Label}' overlapped node '{node.Title}'.");
            }

            for (var j = i + 1; j < layoutList.Count; j++)
            {
                var (edgeB, layoutB) = layoutList[j];
                Assert.False(
                    PillsOverlap(layoutA, layoutB),
                    $"Preset '{presetKey}': edge '{edgeA.Label}' overlapped edge '{edgeB.Label}'.");
            }
        }
    }

    [Fact]
    public void ComputeEdgeLayouts_DenseObstacleOnMidpoint_ShouldNudgeLabelClearAndEnableLeaderLine()
    {
        var flowchartId = Guid.NewGuid();
        var nodeA = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Guard", 2, null, 100, 200, 160, 80);
        var nodeB = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Back", 1, null, 500, 200, 160, 80);
        // Placed directly across the midpoint and curve-slide range between nodeA and nodeB
        var blocker = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Blocker", 3, null, 300, 200, 160, 80);

        var edge = new FlowchartEdgeDto(Guid.NewGuid(), flowchartId, nodeA.Id, nodeB.Id, "Arm Drag to Back", 6, null, 0);

        var layouts = FlowchartLayoutEngine.ComputeEdgeLayouts([nodeA, nodeB, blocker], [edge]);
        var layout = layouts[edge.Id];

        Assert.True(layout.HasLeaderLine, "Expected leader line when label is nudged away from a dense obstacle.");
        Assert.False(PillOverlapsNode(layout, nodeA));
        Assert.False(PillOverlapsNode(layout, nodeB));
        Assert.False(PillOverlapsNode(layout, blocker));
    }

    [Fact]
    public void BuildSvgMarkup_ShouldRenderEdgeLabelsAfterNodesInTopLayerWithInvariantCultureCoordinates()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            var daDk = new CultureInfo("da-DK");
            CultureInfo.CurrentCulture = daDk;
            CultureInfo.CurrentUICulture = daDk;

            var editor = new SvgTestableFlowchartEditor();
            var flowchartId = Guid.NewGuid();
            var nodeA = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Guard", 2, null, 100.5, 200.5, 160, 80);
            var nodeB = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Back", 1, null, 500.5, 200.5, 160, 80);
            var blocker = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Blocker", 3, null, 300.5, 200.5, 160, 80);
            var edge = new FlowchartEdgeDto(Guid.NewGuid(), flowchartId, nodeA.Id, nodeB.Id, "Arm Drag", 6, null, 0);

            editor.SetNodes([nodeA, nodeB, blocker]);
            editor.SetEdges([edge]);

            var svg = editor.TestBuildSvgMarkup();

            var lastNodeGroupCloseIndex = svg.LastIndexOf("  </g>", StringComparison.Ordinal);
            var edgeLabelTextIndex = svg.IndexOf(">Arm Drag</text>", StringComparison.Ordinal);
            var edgeLabelRectIndex = svg.LastIndexOf("rx=\"12\"", StringComparison.Ordinal);
            var leaderLineIndex = svg.IndexOf("<line ", StringComparison.Ordinal);

            Assert.True(lastNodeGroupCloseIndex > 0);
            Assert.True(edgeLabelRectIndex > lastNodeGroupCloseIndex, "Edge label <rect> must be rendered after node <g> blocks.");
            Assert.True(edgeLabelTextIndex > lastNodeGroupCloseIndex, "Edge label <text> must be rendered after node <g> blocks.");
            Assert.True(leaderLineIndex > lastNodeGroupCloseIndex, "Leader <line> must be rendered in the top label layer.");

            // Verify InvariantCulture dot decimals (no Danish comma decimals in SVG attributes)
            Assert.Contains("translate(100.5, 200.5)", svg);
            Assert.DoesNotContain("100,5", svg);
            Assert.DoesNotContain("200,5", svg);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }
}
