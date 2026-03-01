namespace ControlCenter.Api.Models.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Idea;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid SpaceId { get; set; }
    public Space Space { get; set; } = null!;

    public Guid? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }
}

public enum TaskItemStatus
{
    Idea,
    Refinement,
    Requirement,
    Concept,
    InProgress,
    Review,
    Done,
    Cancelled
}
