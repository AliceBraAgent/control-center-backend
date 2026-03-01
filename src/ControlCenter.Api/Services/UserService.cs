using ControlCenter.Api.Data;
using ControlCenter.Api.Models.Dtos;
using ControlCenter.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControlCenter.Api.Services;

public class UserService(ApplicationDbContext db) : IUserService
{
    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        return await db.Users
            .AsNoTracking()
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .Select(u => ToDto(u))
            .ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        return user is null ? null : ToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return ToDto(user);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return null;

        user.Email = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.IsActive = request.IsActive;

        await db.SaveChangesAsync();
        return ToDto(user);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return false;

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return true;
    }

    private static UserDto ToDto(User u) =>
        new(u.Id, u.Email, u.FirstName, u.LastName, u.IsActive, u.CreatedAt);
}
