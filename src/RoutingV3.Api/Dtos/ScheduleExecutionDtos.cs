namespace RoutingV3.Api.Dtos;

public record ScheduleExecutionDto(
    int Id,
    int LineId,
    string LineCode,
    DateOnly Date,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    bool Cancelled);

public record CreateScheduleExecutionRequest(
    int LineId,
    DateOnly Date,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    bool Cancelled);

public record UpdateScheduleExecutionRequest(
    DateTime DepartureTime,
    DateTime ArrivalTime,
    bool Cancelled);
