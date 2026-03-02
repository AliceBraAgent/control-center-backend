using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoutingV3.Api.Dtos;
using RoutingV3.Domain.Models;
using RoutingV3.Infrastructure;

namespace RoutingV3.Api.Controllers;

[ApiController]
[Route("api/v3/routing/hubs")]
public class HubsController : ControllerBase
{
    private readonly RoutingDbContext _db;

    public HubsController(RoutingDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<HubDto>>> GetAll()
    {
        var hubs = await _db.Hubs.OrderBy(h => h.Code).ToListAsync();
        return hubs.Select(h => ToDto(h)).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<HubDto>> GetById(int id)
    {
        var hub = await _db.Hubs.FindAsync(id);
        if (hub == null) return NotFound();
        return ToDto(hub);
    }

    [HttpPost]
    public async Task<ActionResult<HubDto>> Create(CreateHubRequest request)
    {
        var hub = new Hub
        {
            Code = request.Code,
            Name = request.Name,
            Country = request.Country,
            Latitude = request.Latitude,
            Longitude = request.Longitude
        };
        _db.Hubs.Add(hub);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = hub.Id }, ToDto(hub));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<HubDto>> Update(int id, UpdateHubRequest request)
    {
        var hub = await _db.Hubs.FindAsync(id);
        if (hub == null) return NotFound();

        hub.Code = request.Code;
        hub.Name = request.Name;
        hub.Country = request.Country;
        hub.Latitude = request.Latitude;
        hub.Longitude = request.Longitude;

        await _db.SaveChangesAsync();
        return ToDto(hub);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var hub = await _db.Hubs.FindAsync(id);
        if (hub == null) return NotFound();

        _db.Hubs.Remove(hub);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static HubDto ToDto(Hub h) =>
        new(h.Id, h.Code, h.Name, h.Country, h.Latitude, h.Longitude);
}
