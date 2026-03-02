namespace RoutingV3.Domain.Models;

public class Hub
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public ICollection<Line> OriginLines { get; set; } = [];
    public ICollection<Line> DestinationLines { get; set; } = [];
}
