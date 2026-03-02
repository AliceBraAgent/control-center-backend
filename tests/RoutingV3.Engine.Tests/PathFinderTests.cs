using RoutingV3.Domain.Enums;
using RoutingV3.Domain.Models;
using RoutingV3.Engine;

namespace RoutingV3.Engine.Tests;

public class PathFinderTests
{
    private readonly PathFinder _pathFinder;
    private readonly CostCalculator _costCalculator = new();
    private readonly EtaCalculator _etaCalculator = new();

    public PathFinderTests()
    {
        _pathFinder = new PathFinder(_costCalculator, _etaCalculator);
    }

    [Fact]
    public void FindRoutes_DirectRoute_FindsSinglePath()
    {
        // PCA:1 -> HUB-A -> HUB-B -> PCA:2
        var graph = BuildSimpleGraph();
        var monday = new DateTime(2025, 1, 6, 7, 0, 0);

        var routes = _pathFinder.FindRoutes(
            graph,
            originNodeIds: ["pca:1"],
            destinationNodeIds: ["pca:2"],
            mandateCode: "AT-LNZ",
            requiredAttributes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "ADR" },
            earliestDeparture: monday);

        Assert.NotEmpty(routes);
        var bestRoute = routes[0];
        Assert.Equal(3, bestRoute.Path.Count); // Collection + Linehaul + Delivery
    }

    [Fact]
    public void FindRoutes_NoValidMandate_ReturnsEmpty()
    {
        var graph = BuildSimpleGraph();
        var monday = new DateTime(2025, 1, 6, 7, 0, 0);

        var routes = _pathFinder.FindRoutes(
            graph,
            originNodeIds: ["pca:1"],
            destinationNodeIds: ["pca:2"],
            mandateCode: "INVALID-MANDATE",
            requiredAttributes: [],
            earliestDeparture: monday);

        Assert.Empty(routes);
    }

    [Fact]
    public void FindRoutes_MissingAttribute_ReturnsEmpty()
    {
        var graph = BuildSimpleGraph();
        var monday = new DateTime(2025, 1, 6, 7, 0, 0);

        var routes = _pathFinder.FindRoutes(
            graph,
            originNodeIds: ["pca:1"],
            destinationNodeIds: ["pca:2"],
            mandateCode: "AT-LNZ",
            requiredAttributes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "TEMPERATURE_CONTROLLED" },
            earliestDeparture: monday);

        Assert.Empty(routes);
    }

    [Fact]
    public void FindRoutes_MultiHopRoute_FindsPath()
    {
        // PCA:1 -> HUB-A -> HUB-B -> HUB-C -> PCA:2
        var graph = BuildMultiHopGraph();
        var monday = new DateTime(2025, 1, 6, 7, 0, 0);

        var routes = _pathFinder.FindRoutes(
            graph,
            originNodeIds: ["pca:1"],
            destinationNodeIds: ["pca:2"],
            mandateCode: "AT-LNZ",
            requiredAttributes: [],
            earliestDeparture: monday);

        Assert.NotEmpty(routes);
        var bestRoute = routes[0];
        Assert.Equal(4, bestRoute.Path.Count); // Col + LH1 + LH2 + Del
    }

    [Fact]
    public void FindRoutes_OrdersByCost()
    {
        var graph = BuildGraphWithMultipleRoutes();
        var monday = new DateTime(2025, 1, 6, 7, 0, 0);

        var routes = _pathFinder.FindRoutes(
            graph,
            originNodeIds: ["pca:1"],
            destinationNodeIds: ["pca:2"],
            mandateCode: "AT-LNZ",
            requiredAttributes: [],
            earliestDeparture: monday);

        Assert.True(routes.Count >= 2);
        // Results should be ordered by cost
        for (var i = 1; i < routes.Count; i++)
        {
            Assert.True(routes[i].Cost >= routes[i - 1].Cost);
        }
    }

    [Fact]
    public void FindRoutes_RespectsMaxHops()
    {
        // Build a long chain requiring 7 hops
        var graph = BuildLongChainGraph(8);
        var monday = new DateTime(2025, 1, 6, 7, 0, 0);

        var routes = _pathFinder.FindRoutes(
            graph,
            originNodeIds: ["pca:1"],
            destinationNodeIds: ["pca:2"],
            mandateCode: "AT-LNZ",
            requiredAttributes: [],
            earliestDeparture: monday);

        // Should not find a route since 8 hops > max 6
        Assert.Empty(routes);
    }

    [Fact]
    public void FindRoutes_6HopsExact_FindsRoute()
    {
        var graph = BuildLongChainGraph(6);
        var monday = new DateTime(2025, 1, 6, 7, 0, 0);

        var routes = _pathFinder.FindRoutes(
            graph,
            originNodeIds: ["pca:1"],
            destinationNodeIds: ["pca:2"],
            mandateCode: "AT-LNZ",
            requiredAttributes: [],
            earliestDeparture: monday);

        Assert.NotEmpty(routes);
        Assert.Equal(6, routes[0].Path.Count);
    }

    // --- Graph builders ---

    private static RoutingGraph BuildSimpleGraph()
    {
        var graph = new RoutingGraph();

        var pca1 = AddNode(graph, "pca:1", isHub: false);
        var hubA = AddNode(graph, "hub:HUB-A", isHub: true);
        var hubB = AddNode(graph, "hub:HUB-B", isHub: true);
        var pca2 = AddNode(graph, "pca:2", isHub: false);

        AddEdge(graph, "pca:1", "hub:HUB-A", 1, "COL-1", LegType.Collection, 100m);
        AddEdge(graph, "hub:HUB-A", "hub:HUB-B", 2, "LH-1", LegType.Linehaul, 200m);
        AddEdge(graph, "hub:HUB-B", "pca:2", 3, "DEL-1", LegType.Delivery, 150m);

        return graph;
    }

    private static RoutingGraph BuildMultiHopGraph()
    {
        var graph = new RoutingGraph();

        AddNode(graph, "pca:1", isHub: false);
        AddNode(graph, "hub:HUB-A", isHub: true);
        AddNode(graph, "hub:HUB-B", isHub: true);
        AddNode(graph, "hub:HUB-C", isHub: true);
        AddNode(graph, "pca:2", isHub: false);

        AddEdge(graph, "pca:1", "hub:HUB-A", 1, "COL", LegType.Collection, 100m);
        AddEdge(graph, "hub:HUB-A", "hub:HUB-B", 2, "LH-1", LegType.Linehaul, 200m);
        AddEdge(graph, "hub:HUB-B", "hub:HUB-C", 3, "LH-2", LegType.Linehaul, 150m);
        AddEdge(graph, "hub:HUB-C", "pca:2", 4, "DEL", LegType.Delivery, 100m);

        return graph;
    }

    private static RoutingGraph BuildGraphWithMultipleRoutes()
    {
        var graph = new RoutingGraph();

        AddNode(graph, "pca:1", isHub: false);
        AddNode(graph, "hub:HUB-A", isHub: true);
        AddNode(graph, "hub:HUB-B", isHub: true);
        AddNode(graph, "hub:HUB-C", isHub: true);
        AddNode(graph, "pca:2", isHub: false);

        // Route 1: PCA1 -> A -> B -> PCA2 (cost: 100+200+150 = 450)
        AddEdge(graph, "pca:1", "hub:HUB-A", 1, "COL-A", LegType.Collection, 100m);
        AddEdge(graph, "hub:HUB-A", "hub:HUB-B", 2, "LH-AB", LegType.Linehaul, 200m);
        AddEdge(graph, "hub:HUB-B", "pca:2", 3, "DEL-B", LegType.Delivery, 150m);

        // Route 2: PCA1 -> A -> C -> PCA2 (cost: 100+100+100 = 300) — cheaper
        AddEdge(graph, "hub:HUB-A", "hub:HUB-C", 4, "LH-AC", LegType.Linehaul, 100m);
        AddEdge(graph, "hub:HUB-C", "pca:2", 5, "DEL-C", LegType.Delivery, 100m);

        return graph;
    }

    private static RoutingGraph BuildLongChainGraph(int hops)
    {
        var graph = new RoutingGraph();
        AddNode(graph, "pca:1", isHub: false);

        var prevNode = "pca:1";
        for (var i = 0; i < hops - 1; i++)
        {
            var hubNode = $"hub:HUB-{i}";
            AddNode(graph, hubNode, isHub: true);
            AddEdge(graph, prevNode, hubNode, i + 1, $"LEG-{i}",
                i == 0 ? LegType.Collection : LegType.Linehaul, 100m);
            prevNode = hubNode;
        }

        AddNode(graph, "pca:2", isHub: false);
        AddEdge(graph, prevNode, "pca:2", hops, "DEL", LegType.Delivery, 100m);

        return graph;
    }

    private static GraphNode AddNode(RoutingGraph graph, string id, bool isHub)
    {
        var node = new GraphNode { Id = id, IsHub = isHub };
        graph.AddNode(node);
        return node;
    }

    private static void AddEdge(RoutingGraph graph, string from, string to,
        int lineId, string code, LegType type, decimal cost)
    {
        var line = new Line
        {
            Id = lineId,
            Code = code,
            Type = type,
            AttributesJson = "[\"ADR\"]",
            Department = "TEST",
            PricingRef = "test"
        };
        // Add a weekday schedule rule
        line.ScheduleRules.Add(new ScheduleRule
        {
            Id = lineId,
            LineId = lineId,
            DaysOfWeek = "1,2,3,4,5",
            DepartureTime = type switch
            {
                LegType.Collection => new TimeOnly(8, 0),
                LegType.Linehaul => new TimeOnly(18, 0),
                LegType.Delivery => new TimeOnly(6, 0),
                _ => new TimeOnly(8, 0)
            },
            ArrivalTime = type switch
            {
                LegType.Collection => new TimeOnly(18, 0),
                LegType.Linehaul => new TimeOnly(4, 0),
                LegType.Delivery => new TimeOnly(17, 0),
                _ => new TimeOnly(17, 0)
            },
            ArrivalDayOffset = type == LegType.Linehaul ? 1 : 0
        });

        graph.AddEdge(new GraphEdge
        {
            FromNodeId = from,
            ToNodeId = to,
            Line = line,
            BaseCost = cost,
            Attributes = ["ADR"],
            ValidMandateCodes = ["AT-LNZ", "AT-WND"]
        });
    }
}
