namespace RoutingV3.Engine;

/// <summary>
/// Calculates the cost for a route. In production, this would resolve pricing from
/// pricing tables based on weight/volume/zone. For now, uses base costs from edges.
/// </summary>
public class CostCalculator
{
    /// <summary>
    /// Calculate total cost for a path. Handles combined LH+Delivery pricing
    /// by zeroing legs flagged with PricingIncludedInDelivery.
    /// </summary>
    public decimal CalculateRouteCost(List<GraphEdge> path)
    {
        var totalCost = 0m;

        foreach (var edge in path)
        {
            if (edge.Line.PricingIncludedInDelivery)
            {
                // Cost is included in the delivery leg's pricing — zero this leg
                continue;
            }
            totalCost += edge.BaseCost;
        }

        return totalCost;
    }

    /// <summary>
    /// Calculate cost for a single leg.
    /// </summary>
    public decimal CalculateLegCost(GraphEdge edge)
    {
        return edge.Line.PricingIncludedInDelivery ? 0m : edge.BaseCost;
    }
}
