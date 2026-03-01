using ControlCenter.Api.Models.Dtos;

namespace ControlCenter.Api.Services;

public interface ISpaceService
{
    Task<IEnumerable<SpaceDto>> GetAllAsync();
    Task<SpaceDto?> GetByIdAsync(Guid id);
    Task<SpaceDto> CreateAsync(CreateSpaceRequest request);
    Task<SpaceDto?> UpdateAsync(Guid id, UpdateSpaceRequest request);
    Task<bool> DeleteAsync(Guid id);
}
