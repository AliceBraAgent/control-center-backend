namespace RoutingV3.Domain.Models;

public class ScheduleExecution
{
    public int Id { get; set; }
    public int LineId { get; set; }
    public Line Line { get; set; } = null!;

    public DateOnly Date { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public bool Cancelled { get; set; }
}
