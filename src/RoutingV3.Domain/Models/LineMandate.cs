namespace RoutingV3.Domain.Models;

public class LineMandate
{
    public int LineId { get; set; }
    public Line Line { get; set; } = null!;

    public int MandateId { get; set; }
    public Mandate Mandate { get; set; } = null!;
}
