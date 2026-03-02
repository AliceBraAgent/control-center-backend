namespace RoutingV3.Domain.Models;

public class PostalCodeArea
{
    public int Id { get; set; }
    public required string Country { get; set; }
    public required string Pattern { get; set; }
    public string? SubHubCode { get; set; }

    public ICollection<Line> OriginLines { get; set; } = [];
    public ICollection<Line> DestinationLines { get; set; } = [];
}
