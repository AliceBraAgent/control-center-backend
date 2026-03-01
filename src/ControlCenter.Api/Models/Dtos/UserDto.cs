namespace ControlCenter.Api.Models.Dtos;

public record UserDto(Guid Id, string Email, string FirstName, string LastName, bool IsActive, DateTime CreatedAt);

public record CreateUserRequest(string Email, string FirstName, string LastName);

public record UpdateUserRequest(string Email, string FirstName, string LastName, bool IsActive);
