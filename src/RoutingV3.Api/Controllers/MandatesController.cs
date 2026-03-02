using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoutingV3.Api.Dtos;
using RoutingV3.Domain.Models;
using RoutingV3.Infrastructure;

namespace RoutingV3.Api.Controllers;

[ApiController]
[Route("api/v3/routing/mandates")]
public class MandatesController : ControllerBase
{
    private readonly RoutingDbContext _db;

    public MandatesController(RoutingDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<MandateDto>>> GetAll()
    {
        var mandates = await _db.Mandates.OrderBy(m => m.Code).ToListAsync();
        return mandates.Select(m => new MandateDto(m.Id, m.Code, m.Name)).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MandateDto>> GetById(int id)
    {
        var mandate = await _db.Mandates.FindAsync(id);
        if (mandate == null) return NotFound();
        return new MandateDto(mandate.Id, mandate.Code, mandate.Name);
    }

    [HttpPost]
    public async Task<ActionResult<MandateDto>> Create(CreateMandateRequest request)
    {
        var mandate = new Mandate { Code = request.Code, Name = request.Name };
        _db.Mandates.Add(mandate);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = mandate.Id },
            new MandateDto(mandate.Id, mandate.Code, mandate.Name));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MandateDto>> Update(int id, UpdateMandateRequest request)
    {
        var mandate = await _db.Mandates.FindAsync(id);
        if (mandate == null) return NotFound();

        mandate.Code = request.Code;
        mandate.Name = request.Name;
        await _db.SaveChangesAsync();
        return new MandateDto(mandate.Id, mandate.Code, mandate.Name);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var mandate = await _db.Mandates.FindAsync(id);
        if (mandate == null) return NotFound();

        _db.Mandates.Remove(mandate);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
