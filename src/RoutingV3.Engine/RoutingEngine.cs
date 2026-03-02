using RoutingV3.Domain.Enums;
using RoutingV3.Domain.Models;

namespace RoutingV3.Engine;

public record ShipmentRequest(
    LocationDto Origin,
    LocationDto Destination,
    List<ItemDto> Items,
    List<string> Attributes,
    string MandateCode);

public record LocationDto(
    string Country,
    string PostalCode,
    string City,
    string? AddressMatchCode = null);

public record ItemDto(
    int Amount,
    string Type,
    decimal Weight,
    decimal Volume,
    decimal? Length = null,
    decimal? Width = null,
    decimal? Height = null);

public record RoutingResult(List<RoutingOption> Options);

public record RoutingOption(
    List<RouteLeg> Legs,
    decimal TotalCost,
    DateTime ETA,
    DateTime DepartureTime);

public record RouteLeg(
    int Sequence,
    LegType Type,
    int LineId,
    string LineCode,
    string Origin,
    string Destination,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    decimal Cost,
    string? Partner);

/// <summary>
/// Main routing engine orchestrator. Coordinates postal code matching, graph building,
/// path finding, cost, and ETA calculation.
/// </summary>
public class RoutingEngine
{
    private readonly PostalCodeMatcher _postalCodeMatcher;
    private readonly GraphBuilder _graphBuilder;
    private readonly PathFinder _pathFinder;
    private readonly CostCalculator _costCalculator;
    private readonly EtaCalculator _etaCalculator;

    private RoutingGraph? _cachedGraph;
    private IReadOnlyList<PostalCodeArea>? _cachedPostalCodeAreas;

    public RoutingEngine(
        PostalCodeMatcher postalCodeMatcher,
        GraphBuilder graphBuilder,
        PathFinder pathFinder,
        CostCalculator costCalculator,
        EtaCalculator etaCalculator)
    {
        _postalCodeMatcher = postalCodeMatcher;
        _graphBuilder = graphBuilder;
        _pathFinder = pathFinder;
        _costCalculator = costCalculator;
        _etaCalculator = etaCalculator;
    }

    /// <summary>
    /// Load lines and postal code areas, rebuild graph cache.
    /// </summary>
    public void LoadData(IEnumerable<Line> lines, IEnumerable<PostalCodeArea> postalCodeAreas)
    {
        _cachedGraph = _graphBuilder.Build(lines);
        _cachedPostalCodeAreas = postalCodeAreas.ToList();
    }

    public RoutingResult Calculate(ShipmentRequest request, DateTime? departureAfter = null)
    {
        if (_cachedGraph == null || _cachedPostalCodeAreas == null)
            throw new InvalidOperationException("Routing data not loaded. Call LoadData first.");

        var earliestDeparture = departureAfter ?? DateTime.UtcNow;

        // 1. Resolve origin postal code areas
        var originAreas = _postalCodeMatcher.FindMatches(
            request.Origin.Country, request.Origin.PostalCode, _cachedPostalCodeAreas);

        // 2. Resolve destination postal code areas
        var destAreas = _postalCodeMatcher.FindMatches(
            request.Destination.Country, request.Destination.PostalCode, _cachedPostalCodeAreas);

        if (originAreas.Count == 0 && destAreas.Count == 0)
            return new RoutingResult([]);

        // 3. Build origin/destination node IDs
        var originNodeIds = originAreas.Select(a => $"pca:{a.Id}").ToHashSet();
        var destNodeIds = destAreas.Select(a => $"pca:{a.Id}").ToHashSet();

        // Also check if origin/destination are hubs directly (by matching hub codes or postal codes)
        foreach (var node in _cachedGraph.Nodes.Values.Where(n => n.IsHub))
        {
            // Direct hub matching would be done via AddressMatchCode in production
            // For now, origin/dest match only through postal code areas
        }

        if (originNodeIds.Count == 0 || destNodeIds.Count == 0)
            return new RoutingResult([]);

        // 4. Required attributes from shipment
        var requiredAttributes = request.Attributes.ToHashSet(StringComparer.OrdinalIgnoreCase);

        // 5. Find routes
        var routes = _pathFinder.FindRoutes(
            _cachedGraph,
            originNodeIds,
            destNodeIds,
            request.MandateCode,
            requiredAttributes,
            earliestDeparture);

        // 6. Convert to routing options
        var options = routes.Select(r => BuildRoutingOption(r.Path, r.Cost, r.Schedules)).ToList();

        return new RoutingResult(options);
    }

    private static RoutingOption BuildRoutingOption(
        List<GraphEdge> path, decimal totalCost, List<LegSchedule> schedules)
    {
        var legs = new List<RouteLeg>();

        for (var i = 0; i < path.Count; i++)
        {
            var edge = path[i];
            var schedule = i < schedules.Count ? schedules[i] : new LegSchedule(DateTime.MinValue, DateTime.MinValue);
            var legCost = edge.Line.PricingIncludedInDelivery ? 0m : edge.BaseCost;

            legs.Add(new RouteLeg(
                Sequence: i + 1,
                Type: edge.Line.Type,
                LineId: edge.Line.Id,
                LineCode: edge.Line.Code,
                Origin: edge.FromNodeId,
                Destination: edge.ToNodeId,
                DepartureTime: schedule.Departure,
                ArrivalTime: schedule.Arrival,
                Cost: legCost,
                Partner: edge.Line.Partner));
        }

        var departure = schedules.FirstOrDefault()?.Departure ?? DateTime.MinValue;
        var eta = schedules.LastOrDefault()?.Arrival ?? DateTime.MinValue;

        return new RoutingOption(legs, totalCost, eta, departure);
    }
}
