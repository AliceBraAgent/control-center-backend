using System.Text.RegularExpressions;
using ControlCenter.Api.Data;
using ControlCenter.Api.Models.Dtos;
using ControlCenter.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControlCenter.Api.Services;

public partial class DocumentService(ApplicationDbContext db) : IDocumentService
{
    public async Task<IEnumerable<DocumentListDto>> GetAllBySpaceAsync(Guid spaceId)
    {
        return await db.Documents
            .AsNoTracking()
            .Where(d => d.SpaceId == spaceId)
            .OrderBy(d => d.SortOrder)
            .ThenByDescending(d => d.CreatedAt)
            .Select(d => new DocumentListDto(
                d.Id, d.SpaceId, d.Title, d.Slug,
                d.DocumentType, d.SortOrder,
                d.CreatedAt, d.UpdatedAt))
            .ToListAsync();
    }

    public async Task<DocumentDto?> GetByIdAsync(Guid spaceId, Guid id)
    {
        var doc = await db.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id && d.SpaceId == spaceId);

        return doc is null ? null : ToDto(doc);
    }

    public async Task<DocumentDto?> CreateAsync(Guid spaceId, CreateDocumentRequest request)
    {
        var spaceExists = await db.Spaces.AnyAsync(s => s.Id == spaceId);
        if (!spaceExists) return null;

        var slug = GenerateSlug(request.Title);

        // Ensure unique slug within space
        var baseSlug = slug;
        var counter = 1;
        while (await db.Documents.AnyAsync(d => d.SpaceId == spaceId && d.Slug == slug))
        {
            slug = $"{baseSlug}-{counter++}";
        }

        var doc = new Document
        {
            SpaceId = spaceId,
            Title = request.Title,
            Slug = slug,
            Content = request.Content,
            DocumentType = request.DocumentType,
            SortOrder = request.SortOrder,
            CreatedBy = request.CreatedBy,
            UpdatedBy = request.CreatedBy
        };

        db.Documents.Add(doc);
        await db.SaveChangesAsync();

        return ToDto(doc);
    }

    public async Task<DocumentDto?> UpdateAsync(Guid spaceId, Guid id, UpdateDocumentRequest request)
    {
        var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id && d.SpaceId == spaceId);
        if (doc is null) return null;

        doc.Title = request.Title;
        doc.Content = request.Content;
        doc.DocumentType = request.DocumentType;
        doc.SortOrder = request.SortOrder;
        doc.UpdatedBy = request.UpdatedBy;

        await db.SaveChangesAsync();
        return ToDto(doc);
    }

    public async Task<bool> DeleteAsync(Guid spaceId, Guid id)
    {
        var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id && d.SpaceId == spaceId);
        if (doc is null) return false;

        db.Documents.Remove(doc);
        await db.SaveChangesAsync();
        return true;
    }

    private static DocumentDto ToDto(Document d) =>
        new(d.Id, d.SpaceId, d.Title, d.Slug, d.Content,
            d.DocumentType, d.SortOrder,
            d.CreatedAt, d.UpdatedAt, d.CreatedBy, d.UpdatedBy);

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant();
        slug = SlugInvalidChars().Replace(slug, "");
        slug = SlugWhitespace().Replace(slug, "-");
        slug = slug.Trim('-');
        return slug;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex SlugInvalidChars();

    [GeneratedRegex(@"\s+")]
    private static partial Regex SlugWhitespace();
}
