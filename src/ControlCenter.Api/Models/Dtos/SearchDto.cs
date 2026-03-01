namespace ControlCenter.Api.Models.Dtos;

public record SearchResultsDto(
    List<SpaceSearchResult> Spaces,
    List<DocumentSearchResult> Documents,
    List<TaskSearchResult> Tasks);

public record SpaceSearchResult(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentSpaceId);

public record DocumentSearchResult(
    Guid Id,
    string Title,
    string Slug,
    Guid SpaceId,
    string SpaceName,
    string DocumentType);

public record TaskSearchResult(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    Guid SpaceId,
    string SpaceName);
