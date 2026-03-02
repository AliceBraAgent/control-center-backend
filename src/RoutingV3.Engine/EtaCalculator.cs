using RoutingV3.Domain.Models;

namespace RoutingV3.Engine;

public record LegSchedule(DateTime Departure, DateTime Arrival);

/// <summary>
/// Calculates departure/arrival times by chaining schedule rules through route legs.
/// </summary>
public class EtaCalculator
{
    /// <summary>
    /// Compute the schedule for each leg in the path, given the earliest start time.
    /// Each leg must depart after the previous leg arrives.
    /// </summary>
    public List<LegSchedule> CalculateSchedules(List<GraphEdge> path, DateTime earliestStart)
    {
        var schedules = new List<LegSchedule>();
        var currentTime = earliestStart;

        foreach (var edge in path)
        {
            var rules = edge.Line.ScheduleRules.ToList();
            if (rules.Count == 0)
            {
                // No schedule rules — assume immediate transit with 1 hour default
                var departure = currentTime;
                var arrival = currentTime.AddHours(1);
                schedules.Add(new LegSchedule(departure, arrival));
                currentTime = arrival;
                continue;
            }

            var nextDeparture = FindNextDeparture(rules, currentTime);
            if (nextDeparture == null)
            {
                // Fallback: no valid departure found within a week — use current time
                var departure = currentTime;
                var arrival = currentTime.AddHours(1);
                schedules.Add(new LegSchedule(departure, arrival));
                currentTime = arrival;
                continue;
            }

            var (dep, arr) = nextDeparture.Value;
            schedules.Add(new LegSchedule(dep, arr));
            currentTime = arr;
        }

        return schedules;
    }

    /// <summary>
    /// Find the next available departure from schedule rules, starting from the given time.
    /// Searches up to 7 days ahead.
    /// </summary>
    public (DateTime Departure, DateTime Arrival)? FindNextDeparture(
        List<ScheduleRule> rules, DateTime earliestTime)
    {
        // Search up to 7 days ahead
        for (var dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            var checkDate = earliestTime.Date.AddDays(dayOffset);
            var checkDow = checkDate.DayOfWeek;

            foreach (var rule in rules)
            {
                var activeDays = rule.GetDaysOfWeek();
                if (!activeDays.Contains(checkDow)) continue;

                if (rule.DepartureTime == null) continue;

                var departureDateTime = checkDate.Add(rule.DepartureTime.Value.ToTimeSpan());

                // Must depart at or after earliestTime
                if (departureDateTime < earliestTime) continue;

                // Calculate arrival
                DateTime arrivalDateTime;
                if (rule.ArrivalTime != null)
                {
                    arrivalDateTime = checkDate
                        .AddDays(rule.ArrivalDayOffset)
                        .Add(rule.ArrivalTime.Value.ToTimeSpan());
                }
                else
                {
                    // Default: arrive 1 hour after departure
                    arrivalDateTime = departureDateTime.AddHours(1);
                }

                return (departureDateTime, arrivalDateTime);
            }
        }

        return null;
    }
}
