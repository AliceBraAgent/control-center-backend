using ControlCenter.Api.Data;
using ControlCenter.Api.Models.Dtos;
using ControlCenter.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControlCenter.Api.Services;

public class SpaceService(ApplicationDbContext db) : ISpaceService
{
    public async Task<IEnumerable<SpaceDto>> GetAllAsync(Guid? parentId)
    {
        var query = db.Spaces.AsNoTracking()
            .Include(s => s.ParentSpace)
            .AsQueryable();

        query = parentId.HasValue
            ? query.Where(s => s.ParentSpaceId == parentId.Value)
            : query.Where(s => s.ParentSpaceId == null);

        return await query
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SpaceDto(
                s.Id,
                s.Name,
                s.Description,
                s.ParentSpaceId,
                s.ParentSpace != null ? s.ParentSpace.Name : null,
                s.Children.Count,
                s.Documents.Count,
                s.CreatedAt,
                s.UpdatedAt))
            .ToListAsync();
    }

    public async Task<SpaceDetailDto?> GetByIdAsync(Guid id)
    {
        var space = await db.Spaces
            .AsNoTracking()
            .Include(s => s.ParentSpace)
            .Include(s => s.Children)
            .Include(s => s.Documents.OrderBy(d => d.SortOrder))
            .FirstOrDefaultAsync(s => s.Id == id);

        if (space is null) return null;

        var children = space.Children
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new SpaceDto(
                c.Id, c.Name, c.Description,
                c.ParentSpaceId, space.Name,
                0, 0,
                c.CreatedAt, c.UpdatedAt))
            .ToList();

        var documents = space.Documents
            .Select(d => new DocumentDto(
                d.Id, d.SpaceId, d.Title, d.Slug, d.Content,
                d.DocumentType, d.SortOrder,
                d.CreatedAt, d.UpdatedAt, d.CreatedBy, d.UpdatedBy))
            .ToList();

        return new SpaceDetailDto(
            space.Id, space.Name, space.Description,
            space.ParentSpaceId,
            space.ParentSpace?.Name,
            space.Children.Count,
            space.Documents.Count,
            space.CreatedAt, space.UpdatedAt,
            children, documents);
    }

    public async Task<SpaceDto> CreateAsync(CreateSpaceRequest request)
    {
        var space = new Space
        {
            Name = request.Name,
            Description = request.Description,
            ParentSpaceId = request.ParentSpaceId
        };

        db.Spaces.Add(space);
        await db.SaveChangesAsync();

        string? parentName = null;
        if (space.ParentSpaceId.HasValue)
        {
            parentName = await db.Spaces
                .Where(s => s.Id == space.ParentSpaceId.Value)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();
        }

        return new SpaceDto(
            space.Id, space.Name, space.Description,
            space.ParentSpaceId, parentName,
            0, 0,
            space.CreatedAt, space.UpdatedAt);
    }

    public async Task<SpaceDto?> UpdateAsync(Guid id, UpdateSpaceRequest request)
    {
        var space = await db.Spaces.FindAsync(id);
        if (space is null) return null;

        space.Name = request.Name;
        space.Description = request.Description;
        space.ParentSpaceId = request.ParentSpaceId;

        await db.SaveChangesAsync();

        string? parentName = null;
        if (space.ParentSpaceId.HasValue)
        {
            parentName = await db.Spaces
                .Where(s => s.Id == space.ParentSpaceId.Value)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();
        }

        var childCount = await db.Spaces.CountAsync(s => s.ParentSpaceId == id);
        var docCount = await db.Documents.CountAsync(d => d.SpaceId == id);

        return new SpaceDto(
            space.Id, space.Name, space.Description,
            space.ParentSpaceId, parentName,
            childCount, docCount,
            space.CreatedAt, space.UpdatedAt);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var space = await db.Spaces.FindAsync(id);
        if (space is null) return false;

        db.Spaces.Remove(space);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<SpaceDto>> GetChildrenAsync(Guid id)
    {
        var exists = await db.Spaces.AnyAsync(s => s.Id == id);
        if (!exists) return Enumerable.Empty<SpaceDto>();

        return await db.Spaces
            .AsNoTracking()
            .Where(s => s.ParentSpaceId == id)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SpaceDto(
                s.Id, s.Name, s.Description,
                s.ParentSpaceId,
                s.ParentSpace != null ? s.ParentSpace.Name : null,
                s.Children.Count,
                s.Documents.Count,
                s.CreatedAt, s.UpdatedAt))
            .ToListAsync();
    }

    public async Task<IEnumerable<SpaceBreadcrumbDto>> GetPathAsync(Guid id)
    {
        var path = new List<SpaceBreadcrumbDto>();
        var currentId = (Guid?)id;

        while (currentId.HasValue)
        {
            var space = await db.Spaces
                .AsNoTracking()
                .Where(s => s.Id == currentId.Value)
                .Select(s => new { s.Id, s.Name, s.ParentSpaceId })
                .FirstOrDefaultAsync();

            if (space is null) break;

            path.Add(new SpaceBreadcrumbDto(space.Id, space.Name));
            currentId = space.ParentSpaceId;
        }

        path.Reverse();
        return path;
    }

    public async Task<IEnumerable<SpaceRelationDto>> GetRelationsAsync(Guid id)
    {
        return await db.SpaceRelations
            .AsNoTracking()
            .Where(r => r.SourceSpaceId == id || r.TargetSpaceId == id)
            .Select(r => new SpaceRelationDto(
                r.Id,
                r.SourceSpaceId, r.SourceSpace.Name,
                r.TargetSpaceId, r.TargetSpace.Name,
                r.RelationType,
                r.CreatedAt))
            .ToListAsync();
    }

    public async Task<SpaceRelationDto?> CreateRelationAsync(Guid sourceSpaceId, CreateSpaceRelationRequest request)
    {
        var sourceExists = await db.Spaces.AnyAsync(s => s.Id == sourceSpaceId);
        var targetExists = await db.Spaces.AnyAsync(s => s.Id == request.TargetSpaceId);
        if (!sourceExists || !targetExists) return null;

        var relation = new SpaceRelation
        {
            SourceSpaceId = sourceSpaceId,
            TargetSpaceId = request.TargetSpaceId,
            RelationType = request.RelationType
        };

        db.SpaceRelations.Add(relation);
        await db.SaveChangesAsync();

        var sourceName = await db.Spaces.Where(s => s.Id == sourceSpaceId).Select(s => s.Name).FirstAsync();
        var targetName = await db.Spaces.Where(s => s.Id == request.TargetSpaceId).Select(s => s.Name).FirstAsync();

        return new SpaceRelationDto(
            relation.Id,
            relation.SourceSpaceId, sourceName,
            relation.TargetSpaceId, targetName,
            relation.RelationType,
            relation.CreatedAt);
    }

    public async Task<bool> DeleteRelationAsync(Guid spaceId, Guid relationId)
    {
        var relation = await db.SpaceRelations
            .FirstOrDefaultAsync(r => r.Id == relationId
                && (r.SourceSpaceId == spaceId || r.TargetSpaceId == spaceId));

        if (relation is null) return false;

        db.SpaceRelations.Remove(relation);
        await db.SaveChangesAsync();
        return true;
    }
}
