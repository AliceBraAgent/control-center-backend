using RoutingV3.Domain.Enums;
using RoutingV3.Domain.Models;
using RoutingV3.Engine;

namespace RoutingV3.Engine.Tests;

public class CostCalculatorTests
{
    private readonly CostCalculator _calculator = new();

    [Fact]
    public void CalculateRouteCost_EmptyPath_ReturnsZero()
    {
        Assert.Equal(0m, _calculator.CalculateRouteCost([]));
    }

    [Fact]
    public void CalculateRouteCost_SingleLeg_ReturnsBaseCost()
    {
        var path = new List<GraphEdge>
        {
            CreateEdge(100m, pricingIncluded: false)
        };

        Assert.Equal(100m, _calculator.CalculateRouteCost(path));
    }

    [Fact]
    public void CalculateRouteCost_MultiplLegs_SumsCosts()
    {
        var path = new List<GraphEdge>
        {
            CreateEdge(100m, pricingIncluded: false),
            CreateEdge(200m, pricingIncluded: false),
            CreateEdge(150m, pricingIncluded: false)
        };

        Assert.Equal(450m, _calculator.CalculateRouteCost(path));
    }

    [Fact]
    public void CalculateRouteCost_CombinedPricing_ZerosFlaggedLeg()
    {
        // Linehaul priced together with delivery — LH flagged as included
        var path = new List<GraphEdge>
        {
            CreateEdge(100m, pricingIncluded: false),  // Collection
            CreateEdge(200m, pricingIncluded: true),    // Linehaul (included in delivery)
            CreateEdge(300m, pricingIncluded: false)    // Delivery
        };

        // Should be 100 + 0 + 300 = 400
        Assert.Equal(400m, _calculator.CalculateRouteCost(path));
    }

    [Fact]
    public void CalculateLegCost_NormalLeg_ReturnsBaseCost()
    {
        var edge = CreateEdge(250m, pricingIncluded: false);
        Assert.Equal(250m, _calculator.CalculateLegCost(edge));
    }

    [Fact]
    public void CalculateLegCost_IncludedInDelivery_ReturnsZero()
    {
        var edge = CreateEdge(250m, pricingIncluded: true);
        Assert.Equal(0m, _calculator.CalculateLegCost(edge));
    }

    private static GraphEdge CreateEdge(decimal baseCost, bool pricingIncluded) => new()
    {
        FromNodeId = "hub:A",
        ToNodeId = "hub:B",
        Line = new Line
        {
            Id = 1,
            Code = "TEST",
            Department = "TEST",
            PricingRef = "test",
            PricingIncludedInDelivery = pricingIncluded
        },
        BaseCost = baseCost,
        Attributes = ["ADR"],
        ValidMandateCodes = ["AT-LNZ"]
    };
}
