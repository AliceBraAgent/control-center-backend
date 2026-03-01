namespace ControlCenter.Api.Models.Entities;

public class SpaceRelation
{
    public Guid Id { get; set; }
    public Guid SourceSpaceId { get; set; }
    public Guid TargetSpaceId { get; set; }
    public string RelationType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Space SourceSpace { get; set; } = null!;
    public Space TargetSpace { get; set; } = null!;
}
