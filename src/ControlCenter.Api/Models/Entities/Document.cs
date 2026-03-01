namespace ControlCenter.Api.Models.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid SpaceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string DocumentType { get; set; } = "custom";
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public Space Space { get; set; } = null!;
}
