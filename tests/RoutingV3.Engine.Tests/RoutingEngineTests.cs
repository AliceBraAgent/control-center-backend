using RoutingV3.Domain.Enums;
using RoutingV3.Domain.Models;
using RoutingV3.Engine;

namespace RoutingV3.Engine.Tests;

public class RoutingEngineTests
{
    private readonly RoutingEngine _engine;

    public RoutingEngineTests()
    {
        var matcher = new PostalCodeMatcher();
        var builder = new GraphBuilder();
        var costCalc = new CostCalculator();
        var etaCalc = new EtaCalculator();
        var pathFinder = new PathFinder(costCalc, etaCalc);
        _engine = new RoutingEngine(matcher, builder, pathFinder, costCalc, etaCalc);
    }

    [Fact]
    public void Calculate_SimpleATRoute_FindsRoute()
    {
        // Simulate AT-4020 (Linz area) -> AT-1010 (Vienna area)
        // Route: Collection AT-4* -> AT-LNZ (hub) -> Linehaul -> AT-WND (hub) -> Delivery AT-1*
        var (lines, areas) = BuildSeedData();
        _engine.LoadData(lines, areas);

        var request = new ShipmentRequest(
            Origin: new LocationDto("AT", "4020", "Linz"),
            Destination: new LocationDto("AT", "1010", "Wien"),
            Items: [new ItemDto(1, "PALLET", 500, 1.2m)],
            Attributes: ["ADR"],
            MandateCode: "AT-LNZ");

        var monday = new DateTime(2025, 1, 6, 7, 0, 0);
        var result = _engine.Calculate(request, monday);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Options);

        var option = result.Options[0];
        Assert.Equal(3, option.Legs.Count);
        Assert.Equal(LegType.Collection, option.Legs[0].Type);
        Assert.Equal(LegType.Linehaul, option.Legs[1].Type);
        Assert.Equal(LegType.Delivery, option.Legs[2].Type);
    }

    [Fact]
    public void Calculate_CrossBorderATtoDE_FindsRoute()
    {
        // AT-4020 -> DE-40210: Collection -> AT-LNZ -> LH to DE.AUR -> Delivery DE-4*
        var (lines, areas) = BuildSeedData();
        _engine.LoadData(lines, areas);

        var request = new ShipmentRequest(
            Origin: new LocationDto("AT", "4020", "Linz"),
            Destination: new LocationDto("DE", "40210", "Düsseldorf"),
            Items: [new ItemDto(2, "PACKAGE", 25, 0.5m)],
            Attributes: ["ADR"],
            MandateCode: "AT-LNZ");

        var monday = new DateTime(2025, 1, 6, 7, 0, 0);
        var result = _engine.Calculate(request, monday);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Options);

        var option = result.Options[0];
        Assert.True(option.Legs.Count >= 3);
        Assert.Equal(LegType.Collection, option.Legs[0].Type);
        Assert.Equal(LegType.Delivery, option.Legs[^1].Type);
    }

    [Fact]
    public void Calculate_NoMatchingPostalCode_ReturnsEmpty()
    {
        var (lines, areas) = BuildSeedData();
        _engine.LoadData(lines, areas);

        var request = new ShipmentRequest(
            Origin: new LocationDto("FR", "75001", "Paris"),
            Destination: new LocationDto("AT", "1010", "Wien"),
            Items: [new ItemDto(1, "PALLET", 500, 1.2m)],
            Attributes: [],
            MandateCode: "AT-LNZ");

        var result = _engine.Calculate(request, DateTime.Now);
        Assert.Empty(result.Options);
    }

    [Fact]
    public void Calculate_InvalidMandate_ReturnsEmpty()
    {
        var (lines, areas) = BuildSeedData();
        _engine.LoadData(lines, areas);

        var request = new ShipmentRequest(
            Origin: new LocationDto("AT", "4020", "Linz"),
            Destination: new LocationDto("AT", "1010", "Wien"),
            Items: [new ItemDto(1, "PALLET", 500, 1.2m)],
            Attributes: [],
            MandateCode: "NONEXISTENT");

        var monday = new DateTime(2025, 1, 6, 7, 0, 0);
        var result = _engine.Calculate(request, monday);
        Assert.Empty(result.Options);
    }

    [Fact]
    public void Calculate_ResultsOrderedByCost()
    {
        var (lines, areas) = BuildSeedData();
        _engine.LoadData(lines, areas);

        var request = new ShipmentRequest(
            Origin: new LocationDto("AT", "4020", "Linz"),
            Destination: new LocationDto("AT", "1010", "Wien"),
            Items: [new ItemDto(1, "PALLET", 500, 1.2m)],
            Attributes: ["ADR"],
            MandateCode: "AT-LNZ");

        var monday = new DateTime(2025, 1, 6, 7, 0, 0);
        var result = _engine.Calculate(request, monday);

        for (var i = 1; i < result.Options.Count; i++)
        {
            Assert.True(result.Options[i].TotalCost >= result.Options[i - 1].TotalCost);
        }
    }

    [Fact]
    public void Calculate_WithoutLoadData_ThrowsException()
    {
        var request = new ShipmentRequest(
            Origin: new LocationDto("AT", "4020", "Linz"),
            Destination: new LocationDto("AT", "1010", "Wien"),
            Items: [new ItemDto(1, "PALLET", 500, 1.2m)],
            Attributes: [],
            MandateCode: "AT-LNZ");

        Assert.Throws<InvalidOperationException>(() => _engine.Calculate(request));
    }

    private static (List<Line> Lines, List<PostalCodeArea> Areas) BuildSeedData()
    {
        var hubLnz = new Hub { Id = 1, Code = "AT-LNZ", Name = "Linz", Country = "AT" };
        var hubWnd = new Hub { Id = 2, Code = "AT-WND", Name = "Wiener Neudorf", Country = "AT" };
        var hubAur = new Hub { Id = 3, Code = "DE.AUR", Name = "Aurich", Country = "DE" };

        var pcaAT4 = new PostalCodeArea { Id = 1, Country = "AT", Pattern = "4.*" };
        var pcaAT1 = new PostalCodeArea { Id = 2, Country = "AT", Pattern = "1.*" };
        var pcaDE4 = new PostalCodeArea { Id = 3, Country = "DE", Pattern = "4.*", SubHubCode = "DE-41379" };
        var pcaDE1 = new PostalCodeArea { Id = 4, Country = "DE", Pattern = "1.*", SubHubCode = "DE-10115" };

        var mandate1 = new Mandate { Id = 1, Code = "AT-LNZ", Name = "Linz" };
        var mandate2 = new Mandate { Id = 2, Code = "AT-WND", Name = "WND" };

        var weekdays = "1,2,3,4,5";

        var lines = new List<Line>
        {
            CreateLine(1, "NV AT.LNZ-C", LegType.Collection,
                originPca: pcaAT4, destHub: hubLnz, daysOfWeek: weekdays,
                departure: new TimeOnly(8, 0), arrival: new TimeOnly(18, 0), arrivalDayOffset: 0, mandates: [mandate1, mandate2]),

            CreateLine(2, "NV AT.LNZ-D", LegType.Delivery,
                originHub: hubLnz, destPca: pcaAT4, daysOfWeek: weekdays,
                departure: new TimeOnly(6, 0), arrival: new TimeOnly(17, 0), arrivalDayOffset: 0, mandates: [mandate1, mandate2]),

            CreateLine(3, "NV AT.WND-C", LegType.Collection,
                originPca: pcaAT1, destHub: hubWnd, daysOfWeek: weekdays,
                departure: new TimeOnly(8, 0), arrival: new TimeOnly(18, 0), arrivalDayOffset: 0, mandates: [mandate1, mandate2]),

            CreateLine(4, "NV AT.WND-D", LegType.Delivery,
                originHub: hubWnd, destPca: pcaAT1, daysOfWeek: weekdays,
                departure: new TimeOnly(6, 0), arrival: new TimeOnly(17, 0), arrivalDayOffset: 0, mandates: [mandate1, mandate2]),

            CreateLine(5, "AT.LNZ-AT.WND", LegType.Linehaul,
                originHub: hubLnz, destHub: hubWnd, daysOfWeek: weekdays,
                departure: new TimeOnly(18, 0), arrival: new TimeOnly(4, 0), arrivalDayOffset: 1, mandates: [mandate1, mandate2]),

            CreateLine(6, "AT.WND-AT.LNZ", LegType.Linehaul,
                originHub: hubWnd, destHub: hubLnz, daysOfWeek: weekdays,
                departure: new TimeOnly(18, 0), arrival: new TimeOnly(4, 0), arrivalDayOffset: 1, mandates: [mandate1, mandate2]),

            CreateLine(7, "AT.LNZ-DE.AUR", LegType.Linehaul,
                originHub: hubLnz, destHub: hubAur, daysOfWeek: weekdays,
                departure: new TimeOnly(18, 0), arrival: new TimeOnly(1, 0), arrivalDayOffset: 1, mandates: [mandate1, mandate2], partner: "CTL"),

            CreateLine(8, "DE.AUR-DE4-D", LegType.Delivery,
                originHub: hubAur, destPca: pcaDE4, daysOfWeek: weekdays,
                departure: new TimeOnly(1, 0), arrival: new TimeOnly(17, 0), arrivalDayOffset: 0, mandates: [mandate1, mandate2], partner: "CTL"),

            CreateLine(9, "DE.AUR-DE1-D", LegType.Delivery,
                originHub: hubAur, destPca: pcaDE1, daysOfWeek: weekdays,
                departure: new TimeOnly(1, 0), arrival: new TimeOnly(17, 0), arrivalDayOffset: 0, mandates: [mandate1, mandate2], partner: "CTL"),
        };

        var areas = new List<PostalCodeArea> { pcaAT4, pcaAT1, pcaDE4, pcaDE1 };
        return (lines, areas);
    }

    private static Line CreateLine(int id, string code, LegType type,
        PostalCodeArea? originPca = null, Hub? originHub = null,
        PostalCodeArea? destPca = null, Hub? destHub = null,
        string daysOfWeek = "1,2,3,4,5",
        TimeOnly? departure = null, TimeOnly? arrival = null, int arrivalDayOffset = 0,
        List<Mandate>? mandates = null, string? partner = null)
    {
        var line = new Line
        {
            Id = id,
            Code = code,
            Type = type,
            OriginHubId = originHub?.Id,
            OriginHub = originHub,
            OriginPostalCodeAreaId = originPca?.Id,
            OriginPostalCodeArea = originPca,
            DestinationHubId = destHub?.Id,
            DestinationHub = destHub,
            DestinationPostalCodeAreaId = destPca?.Id,
            DestinationPostalCodeArea = destPca,
            AttributesJson = "[\"ADR\"]",
            Department = "TEST",
            Partner = partner,
            PricingRef = "test"
        };

        if (mandates != null)
        {
            foreach (var m in mandates)
                line.LineMandates.Add(new LineMandate { MandateId = m.Id, Mandate = m });
        }

        line.ScheduleRules.Add(new ScheduleRule
        {
            Id = id,
            LineId = id,
            DaysOfWeek = daysOfWeek,
            DepartureTime = departure,
            ArrivalTime = arrival,
            ArrivalDayOffset = arrivalDayOffset
        });

        return line;
    }
}
