namespace ControlCenter.Api.Models.Entities;

public class Space
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = [];
}
