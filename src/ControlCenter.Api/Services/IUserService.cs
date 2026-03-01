using ControlCenter.Api.Models.Dtos;

namespace ControlCenter.Api.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto> CreateAsync(CreateUserRequest request);
    Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteAsync(Guid id);
}
