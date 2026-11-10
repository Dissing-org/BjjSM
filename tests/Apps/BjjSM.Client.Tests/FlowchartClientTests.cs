using System.Text.Json;
using BjjSM.Client.Models;
using BjjSM.Client.Pages;
using Microsoft.AspNetCore.Components.Web;
using Xunit;

namespace BjjSM.Client.Tests;

public class FlowchartClientTests
{
    private class TestableFlowchartEditor : FlowchartEditorPage
    {
        public (string PathData, double LabelX, double LabelY) TestCalculateEdgeGeometry(FlowchartEdgeDto edge)
        {
            return CalculateEdgeGeometry(edge);
        }

        public void SetNodes(List<FlowchartNodeDto> nodes)
        {
            _nodes = nodes;
        }

        public void SetEdges(List<FlowchartEdgeDto> edges)
        {
            _edges = edges;
        }

        public void TestDeleteNode(Guid nodeId)
        {
            DeleteNode(nodeId);
        }

        public void TestDeleteEdge(Guid edgeId)
        {
            DeleteEdge(edgeId);
        }

        public List<FlowchartNodeDto> GetNodes() => _nodes;
        public List<FlowchartEdgeDto> GetEdges() => _edges;

        // Pointer event & touch test accessors
        public Dictionary<long, (double X, double Y)> ActivePointers => _activePointers;
        public double Zoom => _zoom;
        public void SetZoom(double zoom) => _zoom = zoom;
        public double PanX => _panX;
        public double PanY => _panY;
        public bool IsPanning => _isPanning;
        public bool HasDragged => _hasDragged;
        public bool IsDirty => _isDirty;
        public FlowchartNodeDto? SelectedNode => _selectedNode;
        public FlowchartEdgeDto? SelectedEdge => _selectedEdge;
        public FlowchartNodeDto? DraggingNode => _draggingNode;

        public void TriggerCanvasPointerDown(PointerEventArgs e) => OnCanvasPointerDown(e);
        public void TriggerCanvasPointerMove(PointerEventArgs e) => OnCanvasPointerMove(e);
        public void TriggerCanvasPointerUp(PointerEventArgs e) => OnCanvasPointerUp(e);
        public void TriggerCanvasPointerCancel(PointerEventArgs e) => OnCanvasPointerCancel(e);
        public void TriggerNodePointerDown(FlowchartNodeDto node, PointerEventArgs e) => OnNodePointerDown(node, e);
        public void TriggerNodeClick(FlowchartNodeDto node) => HandleNodeClick(node);
        public void TriggerSelectEdge(FlowchartEdgeDto edge) => SelectEdge(edge);
    }

    [Fact]
    public void CalculateEdgeGeometry_HorizontalConnection_ShouldReturnValidPathAndMidpoint()
    {
        var editor = new TestableFlowchartEditor();
        var flowchartId = Guid.NewGuid();

        var nodeA = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Guard", 2, "Bottom", 100, 100, 160, 80);
        var nodeB = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Mount", 1, "Top", 400, 100, 160, 80);

        editor.SetNodes([nodeA, nodeB]);

        var edge = new FlowchartEdgeDto(Guid.NewGuid(), flowchartId, nodeA.Id, nodeB.Id, "Hip Bump", 1, null, 0);

        var (pathData, labelX, labelY) = editor.TestCalculateEdgeGeometry(edge);

        Assert.NotEmpty(pathData);
        Assert.StartsWith("M 260.0 140.0 Q 330.0 140.0 400.0 140.0", pathData);
        Assert.Equal(330, labelX);
        Assert.Equal(140, labelY);
    }

    [Fact]
    public void CalculateEdgeGeometry_WithCurveOffset_ShouldOffsetControlPoint()
    {
        var editor = new TestableFlowchartEditor();
        var flowchartId = Guid.NewGuid();

        var nodeA = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Guard", 2, "Bottom", 100, 100, 160, 80);
        var nodeB = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Mount", 1, "Top", 400, 100, 160, 80);

        editor.SetNodes([nodeA, nodeB]);

        var edgeOffset = new FlowchartEdgeDto(Guid.NewGuid(), flowchartId, nodeA.Id, nodeB.Id, "Flower Sweep", 1, null, 40);

        var (pathData, labelX, labelY) = editor.TestCalculateEdgeGeometry(edgeOffset);

        Assert.NotEmpty(pathData);
        Assert.Equal(330, labelX);
        Assert.True(labelY > 140);
    }

    [Fact]
    public void DeleteNode_ShouldRemoveNodeAndConnectedEdges()
    {
        var editor = new TestableFlowchartEditor();
        var flowchartId = Guid.NewGuid();

        var nodeA = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "A", 1, null, 0, 0);
        var nodeB = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "B", 2, null, 100, 100);
        var edge = new FlowchartEdgeDto(Guid.NewGuid(), flowchartId, nodeA.Id, nodeB.Id, "Pass", 2, null, 0);

        editor.SetNodes([nodeA, nodeB]);
        editor.SetEdges([edge]);

        editor.TestDeleteNode(nodeA.Id);

        Assert.Single(editor.GetNodes());
        Assert.Equal(nodeB.Id, editor.GetNodes().First().Id);
        Assert.Empty(editor.GetEdges());
    }

    [Fact]
    public void UpdateFlowchartRequest_JsonRoundTrip_ShouldPreserveAllAttributes()
    {
        var flowchartId = Guid.NewGuid();
        var node = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Side Control", 1, "Heavy chest pin", 120, 240, 160, 80);
        var targetNode = new FlowchartNodeDto(Guid.NewGuid(), flowchartId, "Kimura Finish", 4, "Keylock grip", 400, 240, 160, 80);
        var edge = new FlowchartEdgeDto(Guid.NewGuid(), flowchartId, node.Id, targetNode.Id, "Kimura Lock", 3, "Isolate wrist", 20);

        var original = new UpdateFlowchartRequest("My System", "System notes", [node, targetNode], [edge]);

        var json = JsonSerializer.Serialize(original, new JsonSerializerOptions { WriteIndented = true });
        var deserialized = JsonSerializer.Deserialize<UpdateFlowchartRequest>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("My System", deserialized.Title);
        Assert.Equal("System notes", deserialized.Description);
        Assert.Equal(2, deserialized.Nodes.Count);
        Assert.Single(deserialized.Edges);
        Assert.Equal("Kimura Lock", deserialized.Edges[0].Label);
        Assert.Equal(20, deserialized.Edges[0].CurveOffset);
    }

    [Fact]
    public void CanvasPointerDown_SinglePointer_ShouldTrackPointerAndInitializeDragSlop()
    {
        var editor = new TestableFlowchartEditor();
        var pointerDown = new PointerEventArgs { PointerId = 1, ClientX = 100, ClientY = 150, Button = 0 };

        editor.TriggerCanvasPointerDown(pointerDown);

        Assert.Single(editor.ActivePointers);
        Assert.Equal((100, 150), editor.ActivePointers[1]);
        Assert.False(editor.HasDragged);
        Assert.True(editor.IsPanning);
    }

    [Fact]
    public void DragSlopThreshold_MovementBelowOrEqual6px_ShouldNotTriggerHasDragged()
    {
        var editor = new TestableFlowchartEditor();
        editor.TriggerCanvasPointerDown(new PointerEventArgs { PointerId = 1, ClientX = 100, ClientY = 100, Button = 0 });

        editor.TriggerCanvasPointerMove(new PointerEventArgs { PointerId = 1, ClientX = 104, ClientY = 100 });

        Assert.False(editor.HasDragged);
    }

    [Fact]
    public void DragSlopThreshold_MovementAbove6px_ShouldTriggerHasDragged()
    {
        var editor = new TestableFlowchartEditor();
        editor.TriggerCanvasPointerDown(new PointerEventArgs { PointerId = 1, ClientX = 100, ClientY = 100, Button = 0 });

        editor.TriggerCanvasPointerMove(new PointerEventArgs { PointerId = 1, ClientX = 110, ClientY = 100 });

        Assert.True(editor.HasDragged);
    }

    [Fact]
    public void NodeDrag_DefersMarkDirtyUntilPointerUp()
    {
        var editor = new TestableFlowchartEditor();
        var node = new FlowchartNodeDto(Guid.NewGuid(), Guid.NewGuid(), "Guard", 1, null, 50, 50, 160, 80);
        editor.SetNodes([node]);

        editor.TriggerNodePointerDown(node, new PointerEventArgs { PointerId = 1, ClientX = 50, ClientY = 50, Button = 0 });
        Assert.False(editor.IsDirty);

        editor.TriggerCanvasPointerMove(new PointerEventArgs { PointerId = 1, ClientX = 70, ClientY = 70 });
        Assert.True(editor.HasDragged);
        Assert.False(editor.IsDirty);

        editor.TriggerCanvasPointerUp(new PointerEventArgs { PointerId = 1, ClientX = 70, ClientY = 70 });
        Assert.True(editor.IsDirty);
    }

    [Fact]
    public void PinchZoom_TwoPointersSpreading_ShouldScaleZoomCorrectly()
    {
        var editor = new TestableFlowchartEditor();
        editor.SetZoom(1.0);

        editor.TriggerCanvasPointerDown(new PointerEventArgs { PointerId = 1, ClientX = 100, ClientY = 100 });
        editor.TriggerCanvasPointerDown(new PointerEventArgs { PointerId = 2, ClientX = 200, ClientY = 100 });

        editor.TriggerCanvasPointerMove(new PointerEventArgs { PointerId = 2, ClientX = 300, ClientY = 100 });

        Assert.Equal(2.0, editor.Zoom, 2);
        Assert.True(editor.HasDragged);
    }

    [Fact]
    public void HandleNodeClick_WhenHasDraggedTrue_ShouldDiscardClickAndResetFlag()
    {
        var editor = new TestableFlowchartEditor();
        var node = new FlowchartNodeDto(Guid.NewGuid(), Guid.NewGuid(), "Guard", 1, null, 50, 50);
        editor.SetNodes([node]);

        editor.TriggerNodePointerDown(node, new PointerEventArgs { PointerId = 1, ClientX = 50, ClientY = 50, Button = 0 });
        editor.TriggerCanvasPointerMove(new PointerEventArgs { PointerId = 1, ClientX = 80, ClientY = 80 });

        Assert.True(editor.HasDragged);

        editor.TriggerNodeClick(node);

        Assert.False(editor.HasDragged);
    }

    [Fact]
    public void SelectEdge_WhenHasDraggedTrue_ShouldIgnoreSelection()
    {
        var editor = new TestableFlowchartEditor();
        var edge = new FlowchartEdgeDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Armbar", 3, null, 0);
        editor.SetEdges([edge]);

        editor.TriggerCanvasPointerDown(new PointerEventArgs { PointerId = 1, ClientX = 10, ClientY = 10, Button = 0 });
        editor.TriggerCanvasPointerMove(new PointerEventArgs { PointerId = 1, ClientX = 50, ClientY = 50 });

        Assert.True(editor.HasDragged);

        editor.TriggerSelectEdge(edge);

        Assert.Null(editor.SelectedEdge);
    }
}
