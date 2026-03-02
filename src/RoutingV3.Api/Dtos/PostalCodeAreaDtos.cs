namespace RoutingV3.Api.Dtos;

public record PostalCodeAreaDto(
    int Id,
    string Country,
    string Pattern,
    string? SubHubCode);

public record CreatePostalCodeAreaRequest(
    string Country,
    string Pattern,
    string? SubHubCode);

public record UpdatePostalCodeAreaRequest(
    string Country,
    string Pattern,
    string? SubHubCode);

public record PostalCodeAreaImportResult(
    int TotalRows,
    int Imported,
    int Skipped,
    List<string> Errors);
