using RoutingV3.Domain.Enums;

namespace RoutingV3.Api.Dtos;

public record LineDto(
    int Id,
    string Code,
    LegType Type,
    int? OriginHubId,
    string? OriginHubCode,
    int? OriginPostalCodeAreaId,
    int? DestinationHubId,
    string? DestinationHubCode,
    int? DestinationPostalCodeAreaId,
    List<string> Attributes,
    List<string> MandateCodes,
    string Department,
    string? Partner,
    string PricingRef,
    bool PricingIncludedInDelivery,
    List<ScheduleRuleDto> ScheduleRules);

public record CreateLineRequest(
    string Code,
    LegType Type,
    int? OriginHubId,
    int? OriginPostalCodeAreaId,
    int? DestinationHubId,
    int? DestinationPostalCodeAreaId,
    List<string> Attributes,
    List<string> MandateCodes,
    string Department,
    string? Partner,
    string PricingRef,
    bool PricingIncludedInDelivery,
    List<CreateScheduleRuleRequest>? ScheduleRules);

public record UpdateLineRequest(
    string Code,
    LegType Type,
    int? OriginHubId,
    int? OriginPostalCodeAreaId,
    int? DestinationHubId,
    int? DestinationPostalCodeAreaId,
    List<string> Attributes,
    List<string> MandateCodes,
    string Department,
    string? Partner,
    string PricingRef,
    bool PricingIncludedInDelivery,
    List<CreateScheduleRuleRequest>? ScheduleRules);

public record ScheduleRuleDto(
    int Id,
    List<DayOfWeek> DaysOfWeek,
    TimeOnly? DepartureTime,
    TimeOnly? ArrivalTime,
    int ArrivalDayOffset);

public record CreateScheduleRuleRequest(
    List<DayOfWeek> DaysOfWeek,
    TimeOnly? DepartureTime,
    TimeOnly? ArrivalTime,
    int ArrivalDayOffset);
