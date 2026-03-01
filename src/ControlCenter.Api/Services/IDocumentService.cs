using ControlCenter.Api.Models.Dtos;

namespace ControlCenter.Api.Services;

public interface IDocumentService
{
    Task<IEnumerable<DocumentListDto>> GetAllBySpaceAsync(Guid spaceId);
    Task<DocumentDto?> GetByIdAsync(Guid spaceId, Guid id);
    Task<DocumentDto?> CreateAsync(Guid spaceId, CreateDocumentRequest request);
    Task<DocumentDto?> UpdateAsync(Guid spaceId, Guid id, UpdateDocumentRequest request);
    Task<bool> DeleteAsync(Guid spaceId, Guid id);
}
