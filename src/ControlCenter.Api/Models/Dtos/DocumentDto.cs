namespace ControlCenter.Api.Models.Dtos;

public record DocumentDto(
    Guid Id,
    Guid SpaceId,
    string Title,
    string Slug,
    string? Content,
    string DocumentType,
    int SortOrder,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy);

public record DocumentListDto(
    Guid Id,
    Guid SpaceId,
    string Title,
    string Slug,
    string DocumentType,
    int SortOrder,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateDocumentRequest(
    string Title,
    string? Content,
    string DocumentType,
    int SortOrder = 0,
    string? CreatedBy = null);

public record UpdateDocumentRequest(
    string Title,
    string? Content,
    string DocumentType,
    int SortOrder = 0,
    string? UpdatedBy = null);
