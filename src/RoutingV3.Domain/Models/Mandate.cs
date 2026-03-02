namespace RoutingV3.Domain.Models;

public class Mandate
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }

    public ICollection<LineMandate> LinesMandates { get; set; } = [];
}
