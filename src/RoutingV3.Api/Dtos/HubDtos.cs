namespace RoutingV3.Api.Dtos;

public record HubDto(
    int Id,
    string Code,
    string Name,
    string Country,
    double? Latitude,
    double? Longitude);

public record CreateHubRequest(
    string Code,
    string Name,
    string Country,
    double? Latitude,
    double? Longitude);

public record UpdateHubRequest(
    string Code,
    string Name,
    string Country,
    double? Latitude,
    double? Longitude);
