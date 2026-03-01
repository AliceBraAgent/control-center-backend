using ControlCenter.Api.Models.Dtos;

namespace ControlCenter.Api.Services;

public interface ISpaceService
{
    Task<IEnumerable<SpaceDto>> GetAllAsync(Guid? parentId);
    Task<SpaceDetailDto?> GetByIdAsync(Guid id);
    Task<SpaceDto> CreateAsync(CreateSpaceRequest request);
    Task<SpaceDto?> UpdateAsync(Guid id, UpdateSpaceRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<SpaceDto>> GetChildrenAsync(Guid id);
    Task<IEnumerable<SpaceBreadcrumbDto>> GetPathAsync(Guid id);
    Task<IEnumerable<SpaceRelationDto>> GetRelationsAsync(Guid id);
    Task<SpaceRelationDto?> CreateRelationAsync(Guid sourceSpaceId, CreateSpaceRelationRequest request);
    Task<bool> DeleteRelationAsync(Guid spaceId, Guid relationId);
}
