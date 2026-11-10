using System.Text.Json;
using BjjSM.Client.Models;
using BjjSM.Client.Pages;
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
        // Normal to horizontal line from left to right points downwards in SVG coords
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
}
