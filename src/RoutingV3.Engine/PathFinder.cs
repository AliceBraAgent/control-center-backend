namespace RoutingV3.Engine;

/// <summary>
/// A candidate path state during search.
/// </summary>
public record PathState(
    string CurrentNodeId,
    List<GraphEdge> Edges,
    decimal TotalCost,
    DateTime CurrentTime,
    int HopCount);

/// <summary>
/// Finds optimal routes through the graph using a time-aware Dijkstra-like search
/// with cost optimization and max hop constraint.
/// </summary>
public class PathFinder
{
    public const int MaxHops = 6;

    private readonly CostCalculator _costCalculator;
    private readonly EtaCalculator _etaCalculator;

    public PathFinder(CostCalculator costCalculator, EtaCalculator etaCalculator)
    {
        _costCalculator = costCalculator;
        _etaCalculator = etaCalculator;
    }

    /// <summary>
    /// Find all valid routes from origin nodes to destination nodes, respecting
    /// mandate, attribute, time, and hop constraints.
    /// </summary>
    public List<(List<GraphEdge> Path, decimal Cost, List<LegSchedule> Schedules)> FindRoutes(
        RoutingGraph graph,
        HashSet<string> originNodeIds,
        HashSet<string> destinationNodeIds,
        string mandateCode,
        HashSet<string> requiredAttributes,
        DateTime earliestDeparture,
        int maxResults = 10)
    {
        var results = new List<(List<GraphEdge> Path, decimal Cost, List<LegSchedule> Schedules)>();

        // Priority queue: (cost, path state)
        var queue = new PriorityQueue<PathState, decimal>();

        // Seed with all origin nodes
        foreach (var originId in originNodeIds)
        {
            if (!graph.Nodes.ContainsKey(originId)) continue;
            queue.Enqueue(
                new PathState(originId, [], 0m, earliestDeparture, 0),
                0m);
        }

        // Track best cost to reach each node to prune dominated paths
        var bestCostToNode = new Dictionary<string, decimal>();

        while (queue.Count > 0 && results.Count < maxResults * 3) // Over-generate to filter
        {
            var state = queue.Dequeue();

            // Check if we reached a destination
            if (destinationNodeIds.Contains(state.CurrentNodeId) && state.Edges.Count > 0)
            {
                var schedules = _etaCalculator.CalculateSchedules(state.Edges, earliestDeparture);
                var cost = _costCalculator.CalculateRouteCost(state.Edges);
                results.Add((state.Edges, cost, schedules));

                if (results.Count >= maxResults * 3) break;
                continue;
            }

            // Max hops check
            if (state.HopCount >= MaxHops) continue;

            // Prune: if we've found a cheaper way to this node, skip
            var stateKey = $"{state.CurrentNodeId}:{state.HopCount}";
            if (bestCostToNode.TryGetValue(stateKey, out var bestCost) && state.TotalCost > bestCost)
                continue;
            bestCostToNode[stateKey] = state.TotalCost;

            // Explore neighbors
            foreach (var edge in graph.GetEdgesFrom(state.CurrentNodeId))
            {
                // Mandate filter: line must serve the shipment's mandate
                if (!edge.ValidMandateCodes.Contains(mandateCode, StringComparer.OrdinalIgnoreCase))
                    continue;

                // Attribute filter: line must have all required attributes
                if (!requiredAttributes.All(attr =>
                    edge.Attributes.Contains(attr, StringComparer.OrdinalIgnoreCase)))
                    continue;

                // Avoid cycles
                if (state.Edges.Any(e => e.Line.Id == edge.Line.Id))
                    continue;

                // Time-aware: check if we can depart on this edge after arriving at this node
                var nextDeparture = _etaCalculator.FindNextDeparture(
                    edge.Line.ScheduleRules.ToList(), state.CurrentTime);

                DateTime arrivalTime;
                if (nextDeparture.HasValue)
                {
                    arrivalTime = nextDeparture.Value.Arrival;
                }
                else
                {
                    // No schedule available within 7 days — skip
                    continue;
                }

                var edgeCost = _costCalculator.CalculateLegCost(edge);
                var newCost = state.TotalCost + edgeCost;
                var newEdges = new List<GraphEdge>(state.Edges) { edge };

                queue.Enqueue(
                    new PathState(edge.ToNodeId, newEdges, newCost, arrivalTime, state.HopCount + 1),
                    newCost);
            }
        }

        // Deduplicate and sort by cost
        return results
            .OrderBy(r => r.Cost)
            .ThenBy(r => r.Schedules.LastOrDefault()?.Arrival ?? DateTime.MaxValue)
            .Take(maxResults)
            .ToList();
    }
}
