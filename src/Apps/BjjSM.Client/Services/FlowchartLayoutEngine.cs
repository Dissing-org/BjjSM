using BjjSM.Client.Models;

namespace BjjSM.Client.Services;

public sealed record EdgeLayout(
    Guid EdgeId,
    string PathData,
    double AnchorX,
    double AnchorY,
    double LabelX,
    double LabelY,
    double PillWidth,
    double PillHeight,
    bool HasLeaderLine);

public static class FlowchartLayoutEngine
{
    private const double NodePadding = 8.0;
    private const double PillPadding = 4.0;
    private const double PillHeight = 24.0;
    private const double MinParallelOffsetSeparation = 45.0;
    private const double LeaderLineThreshold = 14.0;

    private static readonly double[] CurveSlideTs = [0.50, 0.38, 0.62, 0.30, 0.70, 0.22, 0.78];
    private static readonly double[] NormalSearchOffsets = [0, 30, -30, 60, -60, 90, -90, 120, -120, 150, -150, 180, -180, 210, -210, 240, -240];
    private static readonly double[] TangentSearchShifts = [0, 40, -40, 80, -80, 120, -120];

    private readonly record struct Rect(double Left, double Top, double Width, double Height)
    {
        public double Right => Left + Width;
        public double Bottom => Top + Height;

        public bool Overlaps(Rect other) =>
            Left < other.Right &&
            Right > other.Left &&
            Top < other.Bottom &&
            Bottom > other.Top;
    }

    public static IReadOnlyDictionary<Guid, EdgeLayout> ComputeEdgeLayouts(
        IReadOnlyList<FlowchartNodeDto> nodes,
        IReadOnlyList<FlowchartEdgeDto> edges)
    {
        var result = new Dictionary<Guid, EdgeLayout>(edges.Count);
        if (edges.Count == 0)
        {
            return result;
        }

        var nodeById = new Dictionary<Guid, FlowchartNodeDto>(nodes.Count);
        foreach (var node in nodes)
        {
            nodeById[node.Id] = node;
        }

        // Step A: Resolve effective curve offsets for parallel & bidirectional edge groups
        var effectiveOffsets = ResolveEffectiveCurveOffsets(edges);

        // Step C seed: Node bounding boxes padded on all sides
        var obstacles = new List<Rect>(nodes.Count + edges.Count);
        foreach (var node in nodes)
        {
            obstacles.Add(new Rect(
                node.X - NodePadding,
                node.Y - NodePadding,
                node.Width + NodePadding * 2,
                node.Height + NodePadding * 2));
        }

        foreach (var edge in edges)
        {
            var label = edge.Label ?? string.Empty;
            var pillWidth = Math.Max(label.Length * 7.5, 50) + 16;

            if (!nodeById.TryGetValue(edge.SourceNodeId, out var src) ||
                !nodeById.TryGetValue(edge.TargetNodeId, out var tgt))
            {
                result[edge.Id] = new EdgeLayout(edge.Id, string.Empty, 0, 0, 0, 0, pillWidth, PillHeight, false);
                continue;
            }

            var (startX, startY, endX, endY) = ComputeEndpoints(src, tgt);
            var midX = (startX + endX) / 2.0;
            var midY = (startY + endY) / 2.0;

            var dx = endX - startX;
            var dy = endY - startY;
            var length = Math.Sqrt(dx * dx + dy * dy);
            var tanX = length > 0 ? dx / length : 1.0;
            var tanY = length > 0 ? dy / length : 0.0;
            var normX = length > 0 ? -dy / length : 0.0;
            var normY = length > 0 ? dx / length : 1.0;

            var curveOffset = effectiveOffsets[edge.Id];
            var ctrlX = midX + normX * curveOffset;
            var ctrlY = midY + normY * curveOffset;

            var pathData = FormattableString.Invariant(
                $"M {startX:F1} {startY:F1} Q {ctrlX:F1} {ctrlY:F1} {endX:F1} {endY:F1}");

            // Step B & C.1: Try sliding along the quadratic Bezier curve
            var placedOnCurve = false;
            var (anchorX, anchorY) = EvalQuadraticBezier(startX, startY, ctrlX, ctrlY, endX, endY, 0.50);
            var labelX = anchorX;
            var labelY = anchorY;
            var hasLeaderLine = false;

            foreach (var t in CurveSlideTs)
            {
                var (cx, cy) = EvalQuadraticBezier(startX, startY, ctrlX, ctrlY, endX, endY, t);
                var candidateBox = CreatePaddedPillBox(cx, cy, pillWidth, PillHeight);
                if (!HasCollision(candidateBox, obstacles))
                {
                    anchorX = cx;
                    anchorY = cy;
                    labelX = cx;
                    labelY = cy;
                    hasLeaderLine = false;
                    placedOnCurve = true;
                    break;
                }
            }

            // Step C.2: Perpendicular / spiral search around Bezier midpoint
            if (!placedOnCurve)
            {
                var bestDistSq = double.PositiveInfinity;
                var bestX = anchorX;
                var bestY = anchorY;
                var found = false;

                foreach (var nOff in NormalSearchOffsets)
                {
                    foreach (var tShift in TangentSearchShifts)
                    {
                        if (nOff == 0 && tShift == 0)
                        {
                            continue;
                        }

                        var cx = anchorX + normX * nOff + tanX * tShift;
                        var cy = anchorY + normY * nOff + tanY * tShift;
                        var candidateBox = CreatePaddedPillBox(cx, cy, pillWidth, PillHeight);
                        if (!HasCollision(candidateBox, obstacles))
                        {
                            var ddx = cx - anchorX;
                            var ddy = cy - anchorY;
                            var distSq = ddx * ddx + ddy * ddy;
                            if (distSq < bestDistSq)
                            {
                                bestDistSq = distSq;
                                bestX = cx;
                                bestY = cy;
                                found = true;
                            }
                        }
                    }
                }

                if (found)
                {
                    labelX = bestX;
                    labelY = bestY;
                    hasLeaderLine = Math.Sqrt(bestDistSq) > LeaderLineThreshold;
                }
            }

            // Step C.3: Record placed pill box as obstacle for subsequent edges
            obstacles.Add(CreatePaddedPillBox(labelX, labelY, pillWidth, PillHeight));

            result[edge.Id] = new EdgeLayout(
                edge.Id,
                pathData,
                anchorX,
                anchorY,
                labelX,
                labelY,
                pillWidth,
                PillHeight,
                hasLeaderLine);
        }

        return result;
    }

    private static Dictionary<Guid, double> ResolveEffectiveCurveOffsets(IReadOnlyList<FlowchartEdgeDto> edges)
    {
        var effectiveOffsets = new Dictionary<Guid, double>(edges.Count);

        var groups = new Dictionary<(Guid MinId, Guid MaxId), List<FlowchartEdgeDto>>();
        foreach (var edge in edges)
        {
            var key = edge.SourceNodeId.CompareTo(edge.TargetNodeId) <= 0
                ? (edge.SourceNodeId, edge.TargetNodeId)
                : (edge.TargetNodeId, edge.SourceNodeId);

            if (!groups.TryGetValue(key, out var list))
            {
                list = [];
                groups[key] = list;
            }

            list.Add(edge);
        }

        foreach (var group in groups.Values)
        {
            if (group.Count == 1)
            {
                effectiveOffsets[group[0].Id] = group[0].CurveOffset;
                continue;
            }

            var canonicalOffsets = new double[group.Count];
            var hasZeroOffset = false;

            for (var i = 0; i < group.Count; i++)
            {
                var e = group[i];
                var isReverse = e.SourceNodeId.CompareTo(e.TargetNodeId) > 0;
                if (e.CurveOffset == 0)
                {
                    hasZeroOffset = true;
                }

                canonicalOffsets[i] = isReverse ? -e.CurveOffset : e.CurveOffset;
            }

            var needsAutoSeparation = hasZeroOffset || HasClosePair(canonicalOffsets, MinParallelOffsetSeparation);
            if (!needsAutoSeparation)
            {
                foreach (var e in group)
                {
                    effectiveOffsets[e.Id] = e.CurveOffset;
                }
                continue;
            }

            var step = group.Count == 2 ? 110.0 : 90.0;
            var centerIndex = (group.Count - 1) / 2.0;

            for (var i = 0; i < group.Count; i++)
            {
                var e = group[i];
                var isReverse = e.SourceNodeId.CompareTo(e.TargetNodeId) > 0;
                var canonicalOffset = (i - centerIndex) * step;
                effectiveOffsets[e.Id] = isReverse ? -canonicalOffset : canonicalOffset;
            }
        }

        return effectiveOffsets;
    }

    private static bool HasClosePair(double[] values, double threshold)
    {
        var sorted = (double[])values.Clone();
        Array.Sort(sorted);
        for (var i = 1; i < sorted.Length; i++)
        {
            if (Math.Abs(sorted[i] - sorted[i - 1]) < threshold)
            {
                return true;
            }
        }

        return false;
    }

    private static (double StartX, double StartY, double EndX, double EndY) ComputeEndpoints(
        FlowchartNodeDto src,
        FlowchartNodeDto tgt)
    {
        var srcCenterX = src.X + src.Width / 2.0;
        var srcCenterY = src.Y + src.Height / 2.0;
        var tgtCenterX = tgt.X + tgt.Width / 2.0;
        var tgtCenterY = tgt.Y + tgt.Height / 2.0;

        var dx = tgtCenterX - srcCenterX;
        var dy = tgtCenterY - srcCenterY;

        if (Math.Abs(dx) >= Math.Abs(dy))
        {
            return dx >= 0
                ? (src.X + src.Width, srcCenterY, tgt.X, tgtCenterY)
                : (src.X, srcCenterY, tgt.X + tgt.Width, tgtCenterY);
        }

        return dy >= 0
            ? (srcCenterX, src.Y + src.Height, tgtCenterX, tgt.Y)
            : (srcCenterX, src.Y, tgtCenterX, tgt.Y + tgt.Height);
    }

    private static (double X, double Y) EvalQuadraticBezier(
        double startX,
        double startY,
        double ctrlX,
        double ctrlY,
        double endX,
        double endY,
        double t)
    {
        var u = 1.0 - t;
        var x = u * u * startX + 2.0 * u * t * ctrlX + t * t * endX;
        var y = u * u * startY + 2.0 * u * t * ctrlY + t * t * endY;
        return (x, y);
    }

    private static Rect CreatePaddedPillBox(double centerX, double centerY, double pillWidth, double pillHeight) =>
        new(
            centerX - pillWidth / 2.0 - PillPadding,
            centerY - pillHeight / 2.0 - PillPadding,
            pillWidth + PillPadding * 2.0,
            pillHeight + PillPadding * 2.0);

    private static bool HasCollision(Rect candidate, List<Rect> obstacles)
    {
        foreach (var obstacle in obstacles)
        {
            if (candidate.Overlaps(obstacle))
            {
                return true;
            }
        }

        return false;
    }
}
