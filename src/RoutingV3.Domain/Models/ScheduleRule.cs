namespace RoutingV3.Domain.Models;

public class ScheduleRule
{
    public int Id { get; set; }
    public int LineId { get; set; }
    public Line Line { get; set; } = null!;

    // Stored as comma-separated ints (0=Sunday..6=Saturday)
    public required string DaysOfWeek { get; set; }

    public TimeOnly? DepartureTime { get; set; }
    public TimeOnly? ArrivalTime { get; set; }
    public int ArrivalDayOffset { get; set; }

    public List<DayOfWeek> GetDaysOfWeek()
    {
        return DaysOfWeek.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(d => (DayOfWeek)int.Parse(d))
            .ToList();
    }

    public void SetDaysOfWeek(IEnumerable<DayOfWeek> days)
    {
        DaysOfWeek = string.Join(",", days.Select(d => (int)d));
    }
}
