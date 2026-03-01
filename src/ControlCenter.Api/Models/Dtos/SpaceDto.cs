namespace ControlCenter.Api.Models.Dtos;

public record SpaceDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentSpaceId,
    string? ParentSpaceName,
    int ChildCount,
    int DocumentCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record SpaceDetailDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentSpaceId,
    string? ParentSpaceName,
    int ChildCount,
    int DocumentCount,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<SpaceDto> Children,
    List<DocumentDto> Documents);

public record SpaceBreadcrumbDto(Guid Id, string Name);

public record CreateSpaceRequest(string Name, string? Description, Guid? ParentSpaceId);

public record UpdateSpaceRequest(string Name, string? Description, Guid? ParentSpaceId);

public record SpaceRelationDto(
    Guid Id,
    Guid SourceSpaceId,
    string SourceSpaceName,
    Guid TargetSpaceId,
    string TargetSpaceName,
    string RelationType,
    DateTime CreatedAt);

public record CreateSpaceRelationRequest(Guid TargetSpaceId, string RelationType);
