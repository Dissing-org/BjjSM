using BjjSM.Modules.Curriculum.Domain;
using Xunit;

namespace BjjSM.Modules.Curriculum.Domain.Tests;

public class FlowchartDomainTests
{
    private readonly Guid _academyId = Guid.NewGuid();

    [Fact]
    public void Create_ShouldInitializeFlowchartWithEmptyNodesAndEdges()
    {
        var result = Flowchart.Create(_academyId, "My Game Plan", "Test description");

        Assert.True(result.IsSuccess);
        var flowchart = result.Value;
        Assert.Equal("My Game Plan", flowchart.Title);
        Assert.Equal("Test description", flowchart.Description);
        Assert.Empty(flowchart.Nodes);
        Assert.Empty(flowchart.Edges);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenTitleIsEmpty(string title)
    {
        var result = Flowchart.Create(_academyId, title);

        Assert.False(result.IsSuccess);
        Assert.Contains("Title cannot be empty", result.Error);
    }

    [Fact]
    public void AddNode_ShouldAddNodeSuccessfully()
    {
        var fc = Flowchart.Create(_academyId, "Test").Value;

        var result = fc.AddNode("Closed Guard", PositionCategory.Bottom, "Keep posture broken", 100, 200);

        Assert.True(result.IsSuccess);
        Assert.Single(fc.Nodes);
        var node = fc.Nodes.First();
        Assert.Equal("Closed Guard", node.Title);
        Assert.Equal(PositionCategory.Bottom, node.Category);
        Assert.Equal(100, node.X);
        Assert.Equal(200, node.Y);
    }

    [Fact]
    public void UpdateNodePosition_ShouldUpdateCoordinates()
    {
        var fc = Flowchart.Create(_academyId, "Test").Value;
        var node = fc.AddNode("Mount", PositionCategory.Top, null, 50, 50).Value;

        var result = fc.UpdateNodePosition(node.Id, 150, 250);

        Assert.True(result.IsSuccess);
        Assert.Equal(150, node.X);
        Assert.Equal(250, node.Y);
    }

    [Fact]
    public void AddEdge_ShouldLinkExistingNodes()
    {
        var fc = Flowchart.Create(_academyId, "Test").Value;
        var nodeA = fc.AddNode("Guard", PositionCategory.Bottom, null, 0, 0).Value;
        var nodeB = fc.AddNode("Mount", PositionCategory.Top, null, 100, 100).Value;

        var result = fc.AddEdge(nodeA.Id, nodeB.Id, "Hip Bump Sweep", TransitionType.Sweep, "Explosive hip bump", curveOffset: 15);

        Assert.True(result.IsSuccess);
        Assert.Single(fc.Edges);
        var edge = fc.Edges.First();
        Assert.Equal("Hip Bump Sweep", edge.Label);
        Assert.Equal(TransitionType.Sweep, edge.Type);
        Assert.Equal(15, edge.CurveOffset);
    }

    [Fact]
    public void AddEdge_ShouldFail_WhenNodeDoesNotExist()
    {
        var fc = Flowchart.Create(_academyId, "Test").Value;
        var nodeA = fc.AddNode("Guard", PositionCategory.Bottom, null, 0, 0).Value;

        var result = fc.AddEdge(nodeA.Id, Guid.NewGuid(), "Pass", TransitionType.Pass);

        Assert.False(result.IsSuccess);
        Assert.Contains("does not exist", result.Error);
    }

    [Fact]
    public void RemoveNode_ShouldCascadeRemoveIncidentEdges()
    {
        var fc = Flowchart.Create(_academyId, "Test").Value;
        var n1 = fc.AddNode("A", PositionCategory.Top, null, 0, 0).Value;
        var n2 = fc.AddNode("B", PositionCategory.Bottom, null, 10, 10).Value;
        var n3 = fc.AddNode("C", PositionCategory.Submission, null, 20, 20).Value;

        fc.AddEdge(n1.Id, n2.Id, "E1", TransitionType.Transition);
        fc.AddEdge(n2.Id, n3.Id, "E2", TransitionType.SubmissionAttempt);

        Assert.Equal(2, fc.Edges.Count);

        var removeResult = fc.RemoveNode(n2.Id);

        Assert.True(removeResult.IsSuccess);
        Assert.Equal(2, fc.Nodes.Count);
        Assert.Empty(fc.Edges);
    }

    [Fact]
    public void FlowchartPresets_ShouldGenerateValidFlowcharts()
    {
        var summaries = FlowchartPresets.GetPresetSummaries();
        Assert.Equal(3, summaries.Count);

        foreach (var (key, title, _) in summaries)
        {
            var preset = FlowchartPresets.CreatePreset(key, _academyId);
            Assert.NotNull(preset);
            Assert.Equal(title, preset.Title);
            Assert.NotEmpty(preset.Nodes);
            Assert.NotEmpty(preset.Edges);

            var nodeIds = preset.Nodes.Select(n => n.Id).ToHashSet();
            Assert.All(preset.Edges, e =>
            {
                Assert.Contains(e.SourceNodeId, nodeIds);
                Assert.Contains(e.TargetNodeId, nodeIds);
            });
        }
    }
}
