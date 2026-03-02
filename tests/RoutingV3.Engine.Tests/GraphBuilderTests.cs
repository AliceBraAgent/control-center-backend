using RoutingV3.Domain.Enums;
using RoutingV3.Domain.Models;
using RoutingV3.Engine;

namespace RoutingV3.Engine.Tests;

public class GraphBuilderTests
{
    private readonly GraphBuilder _builder = new();

    [Fact]
    public void Build_EmptyLines_ReturnsEmptyGraph()
    {
        var graph = _builder.Build([]);
        Assert.Empty(graph.Nodes);
        Assert.Empty(graph.AdjacencyList);
    }

    [Fact]
    public void Build_SingleLinehaulLine_CreatesTwoNodesAndOneEdge()
    {
        var lines = new List<Line>
        {
            CreateLinehaulLine(1, "LH-1",
                originHub: new Hub { Id = 1, Code = "HUB-A", Name = "A", Country = "AT" },
                destHub: new Hub { Id = 2, Code = "HUB-B", Name = "B", Country = "AT" })
        };

        var graph = _builder.Build(lines);

        Assert.Equal(2, graph.Nodes.Count);
        Assert.Contains("hub:HUB-A", graph.Nodes.Keys);
        Assert.Contains("hub:HUB-B", graph.Nodes.Keys);

        var edges = graph.GetEdgesFrom("hub:HUB-A");
        Assert.Single(edges);
        Assert.Equal("hub:HUB-B", edges[0].ToNodeId);
    }

    [Fact]
    public void Build_CollectionLine_CreatesPostalCodeAreaAndHubNodes()
    {
        var pca = new PostalCodeArea { Id = 1, Country = "AT", Pattern = "4.*" };
        var hub = new Hub { Id = 1, Code = "AT-LNZ", Name = "Linz", Country = "AT" };

        var lines = new List<Line>
        {
            CreateCollectionLine(1, "COL-1", originPca: pca, destHub: hub)
        };

        var graph = _builder.Build(lines);

        Assert.Equal(2, graph.Nodes.Count);
        Assert.Contains("pca:1", graph.Nodes.Keys);
        Assert.Contains("hub:AT-LNZ", graph.Nodes.Keys);

        Assert.False(graph.Nodes["pca:1"].IsHub);
        Assert.True(graph.Nodes["hub:AT-LNZ"].IsHub);
    }

    [Fact]
    public void Build_MultipleLines_SharesNodes()
    {
        var hub = new Hub { Id = 1, Code = "HUB-A", Name = "A", Country = "AT" };
        var hubB = new Hub { Id = 2, Code = "HUB-B", Name = "B", Country = "AT" };
        var hubC = new Hub { Id = 3, Code = "HUB-C", Name = "C", Country = "DE" };

        var lines = new List<Line>
        {
            CreateLinehaulLine(1, "LH-AB", originHub: hub, destHub: hubB),
            CreateLinehaulLine(2, "LH-AC", originHub: hub, destHub: hubC),
        };

        var graph = _builder.Build(lines);

        Assert.Equal(3, graph.Nodes.Count);
        var edgesFromA = graph.GetEdgesFrom("hub:HUB-A");
        Assert.Equal(2, edgesFromA.Count);
    }

    [Fact]
    public void Build_MandatesArePopulated()
    {
        var mandate = new Mandate { Id = 1, Code = "AT-LNZ", Name = "Linz" };
        var hub = new Hub { Id = 1, Code = "HUB-A", Name = "A", Country = "AT" };
        var hubB = new Hub { Id = 2, Code = "HUB-B", Name = "B", Country = "AT" };

        var line = CreateLinehaulLine(1, "LH-1", originHub: hub, destHub: hubB);
        line.LineMandates.Add(new LineMandate { Mandate = mandate });

        var graph = _builder.Build([line]);
        var edges = graph.GetEdgesFrom("hub:HUB-A");

        Assert.Single(edges);
        Assert.Contains("AT-LNZ", edges[0].ValidMandateCodes);
    }

    [Fact]
    public void ParseAttributes_ValidJson()
    {
        var attrs = GraphBuilder.ParseAttributes("[\"ADR\",\"TAILLIFT\"]");
        Assert.Equal(2, attrs.Count);
        Assert.Contains("ADR", attrs);
        Assert.Contains("TAILLIFT", attrs);
    }

    [Fact]
    public void ParseAttributes_EmptyJson()
    {
        var attrs = GraphBuilder.ParseAttributes("[]");
        Assert.Empty(attrs);
    }

    [Fact]
    public void ParseAttributes_InvalidJson_ReturnsEmpty()
    {
        var attrs = GraphBuilder.ParseAttributes("not json");
        Assert.Empty(attrs);
    }

    private static Line CreateLinehaulLine(int id, string code, Hub originHub, Hub destHub) => new()
    {
        Id = id,
        Code = code,
        Type = LegType.Linehaul,
        OriginHubId = originHub.Id,
        OriginHub = originHub,
        DestinationHubId = destHub.Id,
        DestinationHub = destHub,
        AttributesJson = "[\"ADR\"]",
        Department = "EAS",
        PricingRef = "test",
        LineMandates = []
    };

    private static Line CreateCollectionLine(int id, string code, PostalCodeArea originPca, Hub destHub) => new()
    {
        Id = id,
        Code = code,
        Type = LegType.Collection,
        OriginPostalCodeAreaId = originPca.Id,
        OriginPostalCodeArea = originPca,
        DestinationHubId = destHub.Id,
        DestinationHub = destHub,
        AttributesJson = "[\"ADR\"]",
        Department = "Rollfuhr",
        PricingRef = "test",
        LineMandates = []
    };
}
