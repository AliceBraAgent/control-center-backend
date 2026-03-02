using System.Text.Json;
using RoutingV3.Domain.Enums;
using RoutingV3.Domain.Models;

namespace RoutingV3.Engine;

/// <summary>
/// A node in the routing graph. Can be a hub or a virtual postal-code-area node.
/// </summary>
public class GraphNode
{
    public required string Id { get; init; }
    public bool IsHub { get; init; }
    public int? HubId { get; init; }
    public int? PostalCodeAreaId { get; init; }
}

/// <summary>
/// An edge in the routing graph, corresponding to a Line.
/// </summary>
public class GraphEdge
{
    public required string FromNodeId { get; init; }
    public required string ToNodeId { get; init; }
    public required Line Line { get; init; }
    public decimal BaseCost { get; init; }
    public List<string> Attributes { get; init; } = [];
    public List<string> ValidMandateCodes { get; init; } = [];
}

/// <summary>
/// The in-memory routing graph.
/// </summary>
public class RoutingGraph
{
    public Dictionary<string, GraphNode> Nodes { get; } = new();
    public Dictionary<string, List<GraphEdge>> AdjacencyList { get; } = new();

    public void AddNode(GraphNode node)
    {
        Nodes.TryAdd(node.Id, node);
        AdjacencyList.TryAdd(node.Id, []);
    }

    public void AddEdge(GraphEdge edge)
    {
        if (!AdjacencyList.ContainsKey(edge.FromNodeId))
            AdjacencyList[edge.FromNodeId] = [];
        AdjacencyList[edge.FromNodeId].Add(edge);
    }

    public List<GraphEdge> GetEdgesFrom(string nodeId)
    {
        return AdjacencyList.GetValueOrDefault(nodeId, []);
    }
}

public class GraphBuilder
{
    /// <summary>
    /// Builds a routing graph from a set of lines with their related data loaded.
    /// </summary>
    public RoutingGraph Build(IEnumerable<Line> lines)
    {
        var graph = new RoutingGraph();

        foreach (var line in lines)
        {
            var fromNodeId = GetOriginNodeId(line);
            var toNodeId = GetDestinationNodeId(line);

            if (fromNodeId == null || toNodeId == null) continue;

            // Add origin node
            graph.AddNode(CreateNode(line, isOrigin: true, fromNodeId));

            // Add destination node
            graph.AddNode(CreateNode(line, isOrigin: false, toNodeId));

            // Parse attributes
            var attributes = ParseAttributes(line.AttributesJson);

            // Get valid mandate codes
            var mandateCodes = line.LineMandates
                .Select(lm => lm.Mandate.Code)
                .ToList();

            // Add edge
            graph.AddEdge(new GraphEdge
            {
                FromNodeId = fromNodeId,
                ToNodeId = toNodeId,
                Line = line,
                BaseCost = 100m, // Default cost, real pricing would come from pricing tables
                Attributes = attributes,
                ValidMandateCodes = mandateCodes
            });
        }

        return graph;
    }

    public static string? GetOriginNodeId(Line line)
    {
        if (line.OriginHubId.HasValue)
            return $"hub:{line.OriginHub?.Code ?? line.OriginHubId.ToString()}";
        if (line.OriginPostalCodeAreaId.HasValue)
            return $"pca:{line.OriginPostalCodeAreaId}";
        return null;
    }

    public static string? GetDestinationNodeId(Line line)
    {
        if (line.DestinationHubId.HasValue)
            return $"hub:{line.DestinationHub?.Code ?? line.DestinationHubId.ToString()}";
        if (line.DestinationPostalCodeAreaId.HasValue)
            return $"pca:{line.DestinationPostalCodeAreaId}";
        return null;
    }

    private static GraphNode CreateNode(Line line, bool isOrigin, string nodeId)
    {
        if (isOrigin)
        {
            return new GraphNode
            {
                Id = nodeId,
                IsHub = line.OriginHubId.HasValue,
                HubId = line.OriginHubId,
                PostalCodeAreaId = line.OriginPostalCodeAreaId
            };
        }
        return new GraphNode
        {
            Id = nodeId,
            IsHub = line.DestinationHubId.HasValue,
            HubId = line.DestinationHubId,
            PostalCodeAreaId = line.DestinationPostalCodeAreaId
        };
    }

    public static List<string> ParseAttributes(string attributesJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(attributesJson) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
