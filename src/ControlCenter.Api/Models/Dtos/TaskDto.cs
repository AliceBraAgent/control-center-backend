namespace ControlCenter.Api.Models.Dtos;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    Guid SpaceId,
    Guid? AssignedToId,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateTaskRequest(string Title, string? Description, Guid SpaceId, Guid? AssignedToId);

public record UpdateTaskRequest(string Title, string? Description, string Status, Guid? AssignedToId);
