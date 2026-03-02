using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoutingV3.Api.Dtos;
using RoutingV3.Domain.Models;
using RoutingV3.Engine;
using RoutingV3.Infrastructure;

namespace RoutingV3.Api.Controllers;

[ApiController]
[Route("api/v3/routing/lines")]
public class LinesController : ControllerBase
{
    private readonly RoutingDbContext _db;

    public LinesController(RoutingDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<LineDto>>> GetAll(
        [FromQuery] string? mandate = null,
        [FromQuery] string? type = null,
        [FromQuery] string? hub = null,
        [FromQuery] string? attributes = null)
    {
        var query = _db.Lines
            .Include(l => l.OriginHub)
            .Include(l => l.DestinationHub)
            .Include(l => l.LineMandates).ThenInclude(lm => lm.Mandate)
            .Include(l => l.ScheduleRules)
            .AsQueryable();

        if (!string.IsNullOrEmpty(mandate))
            query = query.Where(l => l.LineMandates.Any(lm => lm.Mandate.Code == mandate));

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<Domain.Enums.LegType>(type, true, out var legType))
            query = query.Where(l => l.Type == legType);

        if (!string.IsNullOrEmpty(hub))
            query = query.Where(l =>
                (l.OriginHub != null && l.OriginHub.Code == hub) ||
                (l.DestinationHub != null && l.DestinationHub.Code == hub));

        var lines = await query.OrderBy(l => l.Code).ToListAsync();

        if (!string.IsNullOrEmpty(attributes))
        {
            var requiredAttrs = attributes.Split(',', StringSplitOptions.TrimEntries);
            lines = lines.Where(l =>
            {
                var lineAttrs = GraphBuilder.ParseAttributes(l.AttributesJson);
                return requiredAttrs.All(a => lineAttrs.Contains(a, StringComparer.OrdinalIgnoreCase));
            }).ToList();
        }

        return lines.Select(ToDto).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LineDto>> GetById(int id)
    {
        var line = await LoadLine(id);
        if (line == null) return NotFound();
        return ToDto(line);
    }

    [HttpPost]
    public async Task<ActionResult<LineDto>> Create(CreateLineRequest request)
    {
        var line = new Line
        {
            Code = request.Code,
            Type = request.Type,
            OriginHubId = request.OriginHubId,
            OriginPostalCodeAreaId = request.OriginPostalCodeAreaId,
            DestinationHubId = request.DestinationHubId,
            DestinationPostalCodeAreaId = request.DestinationPostalCodeAreaId,
            AttributesJson = JsonSerializer.Serialize(request.Attributes ?? []),
            Department = request.Department,
            Partner = request.Partner,
            PricingRef = request.PricingRef,
            PricingIncludedInDelivery = request.PricingIncludedInDelivery
        };

        // Add mandates
        if (request.MandateCodes is { Count: > 0 })
        {
            var mandates = await _db.Mandates
                .Where(m => request.MandateCodes.Contains(m.Code))
                .ToListAsync();
            foreach (var m in mandates)
                line.LineMandates.Add(new LineMandate { Mandate = m });
        }

        // Add schedule rules
        if (request.ScheduleRules != null)
        {
            foreach (var sr in request.ScheduleRules)
            {
                var rule = new ScheduleRule
                {
                    DaysOfWeek = string.Join(",", sr.DaysOfWeek.Select(d => (int)d)),
                    DepartureTime = sr.DepartureTime,
                    ArrivalTime = sr.ArrivalTime,
                    ArrivalDayOffset = sr.ArrivalDayOffset
                };
                line.ScheduleRules.Add(rule);
            }
        }

        _db.Lines.Add(line);
        await _db.SaveChangesAsync();

        var created = await LoadLine(line.Id);
        return CreatedAtAction(nameof(GetById), new { id = line.Id }, ToDto(created!));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LineDto>> Update(int id, UpdateLineRequest request)
    {
        var line = await _db.Lines
            .Include(l => l.LineMandates)
            .Include(l => l.ScheduleRules)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (line == null) return NotFound();

        line.Code = request.Code;
        line.Type = request.Type;
        line.OriginHubId = request.OriginHubId;
        line.OriginPostalCodeAreaId = request.OriginPostalCodeAreaId;
        line.DestinationHubId = request.DestinationHubId;
        line.DestinationPostalCodeAreaId = request.DestinationPostalCodeAreaId;
        line.AttributesJson = JsonSerializer.Serialize(request.Attributes ?? []);
        line.Department = request.Department;
        line.Partner = request.Partner;
        line.PricingRef = request.PricingRef;
        line.PricingIncludedInDelivery = request.PricingIncludedInDelivery;

        // Update mandates
        _db.LineMandates.RemoveRange(line.LineMandates);
        if (request.MandateCodes is { Count: > 0 })
        {
            var mandates = await _db.Mandates
                .Where(m => request.MandateCodes.Contains(m.Code))
                .ToListAsync();
            foreach (var m in mandates)
                line.LineMandates.Add(new LineMandate { MandateId = m.Id });
        }

        // Update schedule rules
        _db.ScheduleRules.RemoveRange(line.ScheduleRules);
        if (request.ScheduleRules != null)
        {
            foreach (var sr in request.ScheduleRules)
            {
                line.ScheduleRules.Add(new ScheduleRule
                {
                    DaysOfWeek = string.Join(",", sr.DaysOfWeek.Select(d => (int)d)),
                    DepartureTime = sr.DepartureTime,
                    ArrivalTime = sr.ArrivalTime,
                    ArrivalDayOffset = sr.ArrivalDayOffset
                });
            }
        }

        await _db.SaveChangesAsync();

        var updated = await LoadLine(id);
        return ToDto(updated!);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var line = await _db.Lines.FindAsync(id);
        if (line == null) return NotFound();

        _db.Lines.Remove(line);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<Line?> LoadLine(int id)
    {
        return await _db.Lines
            .Include(l => l.OriginHub)
            .Include(l => l.DestinationHub)
            .Include(l => l.LineMandates).ThenInclude(lm => lm.Mandate)
            .Include(l => l.ScheduleRules)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    private static LineDto ToDto(Line l)
    {
        return new LineDto(
            l.Id,
            l.Code,
            l.Type,
            l.OriginHubId,
            l.OriginHub?.Code,
            l.OriginPostalCodeAreaId,
            l.DestinationHubId,
            l.DestinationHub?.Code,
            l.DestinationPostalCodeAreaId,
            GraphBuilder.ParseAttributes(l.AttributesJson),
            l.LineMandates.Select(lm => lm.Mandate.Code).ToList(),
            l.Department,
            l.Partner,
            l.PricingRef,
            l.PricingIncludedInDelivery,
            l.ScheduleRules.Select(sr => new ScheduleRuleDto(
                sr.Id,
                sr.GetDaysOfWeek(),
                sr.DepartureTime,
                sr.ArrivalTime,
                sr.ArrivalDayOffset)).ToList());
    }
}
