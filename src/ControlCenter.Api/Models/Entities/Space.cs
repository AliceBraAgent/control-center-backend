namespace ControlCenter.Api.Models.Entities;

public class Space
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentSpaceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Space? ParentSpace { get; set; }
    public ICollection<Space> Children { get; set; } = [];
    public ICollection<TaskItem> Tasks { get; set; } = [];
    public ICollection<Document> Documents { get; set; } = [];
    public ICollection<SpaceRelation> SourceRelations { get; set; } = [];
    public ICollection<SpaceRelation> TargetRelations { get; set; } = [];
}
