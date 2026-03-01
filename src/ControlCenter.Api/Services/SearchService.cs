using ControlCenter.Api.Data;
using ControlCenter.Api.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ControlCenter.Api.Services;

public class SearchService(ApplicationDbContext db) : ISearchService
{
    public async Task<SearchResultsDto> SearchAsync(string query)
    {
        var term = query.Trim().ToLower();

        var spaces = await db.Spaces
            .AsNoTracking()
            .Where(s => s.Name.ToLower().Contains(term)
                     || (s.Description != null && s.Description.ToLower().Contains(term)))
            .OrderBy(s => s.Name)
            .Take(20)
            .Select(s => new SpaceSearchResult(s.Id, s.Name, s.Description, s.ParentSpaceId))
            .ToListAsync();

        var documents = await db.Documents
            .AsNoTracking()
            .Include(d => d.Space)
            .Where(d => d.Title.ToLower().Contains(term)
                     || (d.Content != null && d.Content.ToLower().Contains(term)))
            .OrderBy(d => d.Title)
            .Take(20)
            .Select(d => new DocumentSearchResult(
                d.Id, d.Title, d.Slug, d.SpaceId, d.Space.Name, d.DocumentType))
            .ToListAsync();

        var tasks = await db.Tasks
            .AsNoTracking()
            .Include(t => t.Space)
            .Where(t => t.Title.ToLower().Contains(term)
                     || (t.Description != null && t.Description.ToLower().Contains(term)))
            .OrderBy(t => t.Title)
            .Take(20)
            .Select(t => new TaskSearchResult(
                t.Id, t.Title, t.Description, t.Status.ToString(), t.SpaceId, t.Space.Name))
            .ToListAsync();

        return new SearchResultsDto(spaces, documents, tasks);
    }
}
