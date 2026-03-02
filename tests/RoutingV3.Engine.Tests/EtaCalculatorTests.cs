using RoutingV3.Domain.Models;
using RoutingV3.Engine;

namespace RoutingV3.Engine.Tests;

public class EtaCalculatorTests
{
    private readonly EtaCalculator _calculator = new();

    [Fact]
    public void FindNextDeparture_SameDay_ReturnsSameDaySchedule()
    {
        var rules = new List<ScheduleRule>
        {
            CreateWeekdayRule(new TimeOnly(18, 0), new TimeOnly(4, 0), arrivalDayOffset: 1)
        };

        // Monday 10:00 — next departure is Monday 18:00
        var monday = new DateTime(2025, 1, 6, 10, 0, 0);
        var result = _calculator.FindNextDeparture(rules, monday);

        Assert.NotNull(result);
        Assert.Equal(new DateTime(2025, 1, 6, 18, 0, 0), result.Value.Departure);
        Assert.Equal(new DateTime(2025, 1, 7, 4, 0, 0), result.Value.Arrival);
    }

    [Fact]
    public void FindNextDeparture_AfterDepartureTime_SkipsToNextDay()
    {
        var rules = new List<ScheduleRule>
        {
            CreateWeekdayRule(new TimeOnly(18, 0), new TimeOnly(4, 0), arrivalDayOffset: 1)
        };

        // Monday 19:00 — missed the 18:00 departure, next is Tuesday 18:00
        var monday = new DateTime(2025, 1, 6, 19, 0, 0);
        var result = _calculator.FindNextDeparture(rules, monday);

        Assert.NotNull(result);
        Assert.Equal(new DateTime(2025, 1, 7, 18, 0, 0), result.Value.Departure);
    }

    [Fact]
    public void FindNextDeparture_Weekend_SkipsToMonday()
    {
        var rules = new List<ScheduleRule>
        {
            CreateWeekdayRule(new TimeOnly(18, 0), new TimeOnly(4, 0), arrivalDayOffset: 1)
        };

        // Saturday 10:00 — next weekday is Monday
        var saturday = new DateTime(2025, 1, 4, 10, 0, 0);
        var result = _calculator.FindNextDeparture(rules, saturday);

        Assert.NotNull(result);
        Assert.Equal(DayOfWeek.Monday, result.Value.Departure.DayOfWeek);
        Assert.Equal(new DateTime(2025, 1, 6, 18, 0, 0), result.Value.Departure);
    }

    [Fact]
    public void FindNextDeparture_NoRules_ReturnsNull()
    {
        var result = _calculator.FindNextDeparture([], DateTime.Now);
        Assert.Null(result);
    }

    [Fact]
    public void CalculateSchedules_ChainsDeparturesCorrectly()
    {
        var line1 = CreateLineWithSchedule(1, new TimeOnly(8, 0), new TimeOnly(18, 0), 0);
        var line2 = CreateLineWithSchedule(2, new TimeOnly(18, 0), new TimeOnly(4, 0), 1);
        var line3 = CreateLineWithSchedule(3, new TimeOnly(6, 0), new TimeOnly(17, 0), 0);

        var edges = new List<GraphEdge>
        {
            CreateEdgeWithLine(line1),
            CreateEdgeWithLine(line2),
            CreateEdgeWithLine(line3)
        };

        // Monday 7:00 start
        var monday = new DateTime(2025, 1, 6, 7, 0, 0);
        var schedules = _calculator.CalculateSchedules(edges, monday);

        Assert.Equal(3, schedules.Count);

        // Leg 1: departs Mon 08:00, arrives Mon 18:00
        Assert.Equal(new DateTime(2025, 1, 6, 8, 0, 0), schedules[0].Departure);
        Assert.Equal(new DateTime(2025, 1, 6, 18, 0, 0), schedules[0].Arrival);

        // Leg 2: departs Mon 18:00, arrives Tue 04:00
        Assert.Equal(new DateTime(2025, 1, 6, 18, 0, 0), schedules[1].Departure);
        Assert.Equal(new DateTime(2025, 1, 7, 4, 0, 0), schedules[1].Arrival);

        // Leg 3: departs Tue 06:00, arrives Tue 17:00
        Assert.Equal(new DateTime(2025, 1, 7, 6, 0, 0), schedules[2].Departure);
        Assert.Equal(new DateTime(2025, 1, 7, 17, 0, 0), schedules[2].Arrival);
    }

    [Fact]
    public void CalculateSchedules_EmptyPath_ReturnsEmpty()
    {
        var schedules = _calculator.CalculateSchedules([], DateTime.Now);
        Assert.Empty(schedules);
    }

    private static ScheduleRule CreateWeekdayRule(TimeOnly departure, TimeOnly arrival, int arrivalDayOffset)
    {
        var rule = new ScheduleRule
        {
            Id = 1,
            LineId = 1,
            DaysOfWeek = "1,2,3,4,5", // Mon-Fri
            DepartureTime = departure,
            ArrivalTime = arrival,
            ArrivalDayOffset = arrivalDayOffset
        };
        return rule;
    }

    private static Line CreateLineWithSchedule(int id, TimeOnly departure, TimeOnly arrival, int arrivalDayOffset)
    {
        var line = new Line
        {
            Id = id,
            Code = $"TEST-{id}",
            Department = "TEST",
            PricingRef = "test"
        };
        line.ScheduleRules.Add(new ScheduleRule
        {
            Id = id,
            LineId = id,
            DaysOfWeek = "1,2,3,4,5",
            DepartureTime = departure,
            ArrivalTime = arrival,
            ArrivalDayOffset = arrivalDayOffset
        });
        return line;
    }

    private static GraphEdge CreateEdgeWithLine(Line line) => new()
    {
        FromNodeId = "hub:A",
        ToNodeId = "hub:B",
        Line = line,
        BaseCost = 100m,
        Attributes = [],
        ValidMandateCodes = []
    };
}
