using ControlCenter.Api.Models.Dtos;

namespace ControlCenter.Api.Services;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllAsync();
    Task<TaskDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaskDto>> GetBySpaceIdAsync(Guid spaceId);
    Task<TaskDto> CreateAsync(CreateTaskRequest request);
    Task<TaskDto?> UpdateAsync(Guid id, UpdateTaskRequest request);
    Task<bool> DeleteAsync(Guid id);
}
