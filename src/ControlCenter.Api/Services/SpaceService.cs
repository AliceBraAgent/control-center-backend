using ControlCenter.Api.Data;
using ControlCenter.Api.Models.Dtos;
using ControlCenter.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControlCenter.Api.Services;

public class SpaceService(ApplicationDbContext db) : ISpaceService
{
    public async Task<IEnumerable<SpaceDto>> GetAllAsync()
    {
        return await db.Spaces
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => ToDto(s))
            .ToListAsync();
    }

    public async Task<SpaceDto?> GetByIdAsync(Guid id)
    {
        var space = await db.Spaces.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        return space is null ? null : ToDto(space);
    }

    public async Task<SpaceDto> CreateAsync(CreateSpaceRequest request)
    {
        var space = new Space
        {
            Name = request.Name,
            Description = request.Description
        };

        db.Spaces.Add(space);
        await db.SaveChangesAsync();

        return ToDto(space);
    }

    public async Task<SpaceDto?> UpdateAsync(Guid id, UpdateSpaceRequest request)
    {
        var space = await db.Spaces.FindAsync(id);
        if (space is null) return null;

        space.Name = request.Name;
        space.Description = request.Description;

        await db.SaveChangesAsync();
        return ToDto(space);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var space = await db.Spaces.FindAsync(id);
        if (space is null) return false;

        db.Spaces.Remove(space);
        await db.SaveChangesAsync();
        return true;
    }

    private static SpaceDto ToDto(Space s) =>
        new(s.Id, s.Name, s.Description, s.CreatedAt, s.UpdatedAt);
}
