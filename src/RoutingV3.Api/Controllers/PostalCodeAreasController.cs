using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoutingV3.Api.Dtos;
using RoutingV3.Domain.Models;
using RoutingV3.Infrastructure;

namespace RoutingV3.Api.Controllers;

[ApiController]
[Route("api/v3/routing/postal-code-areas")]
public class PostalCodeAreasController : ControllerBase
{
    private readonly RoutingDbContext _db;

    public PostalCodeAreasController(RoutingDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<PostalCodeAreaDto>>> GetAll(
        [FromQuery] string? country = null)
    {
        var query = _db.PostalCodeAreas.AsQueryable();
        if (!string.IsNullOrEmpty(country))
            query = query.Where(p => p.Country == country);

        var areas = await query.OrderBy(p => p.Country).ThenBy(p => p.Pattern).ToListAsync();
        return areas.Select(a => ToDto(a)).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostalCodeAreaDto>> GetById(int id)
    {
        var area = await _db.PostalCodeAreas.FindAsync(id);
        if (area == null) return NotFound();
        return ToDto(area);
    }

    [HttpPost]
    public async Task<ActionResult<PostalCodeAreaDto>> Create(CreatePostalCodeAreaRequest request)
    {
        var area = new PostalCodeArea
        {
            Country = request.Country,
            Pattern = request.Pattern,
            SubHubCode = request.SubHubCode
        };
        _db.PostalCodeAreas.Add(area);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = area.Id }, ToDto(area));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PostalCodeAreaDto>> Update(int id, UpdatePostalCodeAreaRequest request)
    {
        var area = await _db.PostalCodeAreas.FindAsync(id);
        if (area == null) return NotFound();

        area.Country = request.Country;
        area.Pattern = request.Pattern;
        area.SubHubCode = request.SubHubCode;
        await _db.SaveChangesAsync();
        return ToDto(area);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var area = await _db.PostalCodeAreas.FindAsync(id);
        if (area == null) return NotFound();

        _db.PostalCodeAreas.Remove(area);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Import postal code areas from CSV.
    /// Expected format: Country,Pattern,SubHubCode (with header row)
    /// </summary>
    [HttpPost("import")]
    public async Task<ActionResult<PostalCodeAreaImportResult>> Import(IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest("Empty file");

        var errors = new List<string>();
        var imported = 0;
        var skipped = 0;
        var totalRows = 0;

        using var reader = new StreamReader(file.OpenReadStream());
        var headerLine = await reader.ReadLineAsync();
        if (headerLine == null)
            return BadRequest("Empty file");

        while (await reader.ReadLineAsync() is { } line)
        {
            totalRows++;
            var parts = line.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length < 2)
            {
                errors.Add($"Row {totalRows}: insufficient columns");
                skipped++;
                continue;
            }

            var country = parts[0];
            var pattern = parts[1];
            var subHubCode = parts.Length > 2 ? parts[2] : null;

            if (string.IsNullOrWhiteSpace(country) || string.IsNullOrWhiteSpace(pattern))
            {
                errors.Add($"Row {totalRows}: missing country or pattern");
                skipped++;
                continue;
            }

            // Check for duplicate
            var exists = await _db.PostalCodeAreas
                .AnyAsync(p => p.Country == country && p.Pattern == pattern);
            if (exists)
            {
                skipped++;
                continue;
            }

            _db.PostalCodeAreas.Add(new PostalCodeArea
            {
                Country = country,
                Pattern = pattern,
                SubHubCode = string.IsNullOrWhiteSpace(subHubCode) ? null : subHubCode
            });
            imported++;
        }

        await _db.SaveChangesAsync();

        return new PostalCodeAreaImportResult(totalRows, imported, skipped, errors);
    }

    private static PostalCodeAreaDto ToDto(PostalCodeArea a) =>
        new(a.Id, a.Country, a.Pattern, a.SubHubCode);
}
