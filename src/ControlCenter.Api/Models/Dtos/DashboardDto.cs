namespace ControlCenter.Api.Models.Dtos;

public record DashboardStatsDto(
    int TotalSpaces,
    int TotalTasks,
    Dictionary<string, int> TasksByStatus,
    int CompletedTasks,
    int TotalDocuments,
    List<RecentTaskDto> RecentTasks,
    List<RecentDocumentDto> RecentDocuments);

public record RecentTaskDto(
    Guid Id,
    string Title,
    string Status,
    Guid SpaceId,
    string SpaceName,
    DateTime CreatedAt);

public record RecentDocumentDto(
    Guid Id,
    string Title,
    Guid SpaceId,
    string SpaceName,
    DateTime CreatedAt);
