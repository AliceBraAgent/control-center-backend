namespace ControlCenter.Api.Models.Dtos;

public record SpaceDto(Guid Id, string Name, string? Description, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateSpaceRequest(string Name, string? Description);

public record UpdateSpaceRequest(string Name, string? Description);
