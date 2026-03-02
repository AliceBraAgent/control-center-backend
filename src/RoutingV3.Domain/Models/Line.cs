using RoutingV3.Domain.Enums;

namespace RoutingV3.Domain.Models;

public class Line
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public LegType Type { get; set; }

    // Origin endpoint - either a hub or postal code area
    public int? OriginHubId { get; set; }
    public Hub? OriginHub { get; set; }
    public int? OriginPostalCodeAreaId { get; set; }
    public PostalCodeArea? OriginPostalCodeArea { get; set; }

    // Destination endpoint - either a hub or postal code area
    public int? DestinationHubId { get; set; }
    public Hub? DestinationHub { get; set; }
    public int? DestinationPostalCodeAreaId { get; set; }
    public PostalCodeArea? DestinationPostalCodeArea { get; set; }

    // Attributes as comma-separated string in DB, exposed as list
    public string AttributesJson { get; set; } = "[]";

    public required string Department { get; set; }
    public string? Partner { get; set; }
    public required string PricingRef { get; set; }
    public bool PricingIncludedInDelivery { get; set; }

    public ICollection<LineMandate> LineMandates { get; set; } = [];
    public ICollection<ScheduleRule> ScheduleRules { get; set; } = [];
    public ICollection<ScheduleExecution> ScheduleExecutions { get; set; } = [];
}
