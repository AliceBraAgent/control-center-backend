using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoutingV3.Api.Dtos;
using RoutingV3.Domain.Models;
using RoutingV3.Infrastructure;

namespace RoutingV3.Api.Controllers;

[ApiController]
[Route("api/v3/routing/schedules")]
public class SchedulesController : ControllerBase
{
    private readonly RoutingDbContext _db;

    public SchedulesController(RoutingDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<ScheduleExecutionDto>>> GetAll(
        [FromQuery] int? lineId = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null)
    {
        var query = _db.ScheduleExecutions
            .Include(s => s.Line)
            .AsQueryable();

        if (lineId.HasValue)
            query = query.Where(s => s.LineId == lineId.Value);
        if (from.HasValue)
            query = query.Where(s => s.Date >= from.Value);
        if (to.HasValue)
            query = query.Where(s => s.Date <= to.Value);

        var executions = await query.OrderBy(s => s.Date).ThenBy(s => s.DepartureTime).ToListAsync();
        return executions.Select(ToDto).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ScheduleExecutionDto>> GetById(int id)
    {
        var exec = await _db.ScheduleExecutions.Include(s => s.Line).FirstOrDefaultAsync(s => s.Id == id);
        if (exec == null) return NotFound();
        return ToDto(exec);
    }

    [HttpPost]
    public async Task<ActionResult<ScheduleExecutionDto>> Create(CreateScheduleExecutionRequest request)
    {
        var line = await _db.Lines.FindAsync(request.LineId);
        if (line == null) return BadRequest("Line not found");

        var exec = new ScheduleExecution
        {
            LineId = request.LineId,
            Date = request.Date,
            DepartureTime = request.DepartureTime,
            ArrivalTime = request.ArrivalTime,
            Cancelled = request.Cancelled
        };
        _db.ScheduleExecutions.Add(exec);
        await _db.SaveChangesAsync();

        exec.Line = line;
        return CreatedAtAction(nameof(GetById), new { id = exec.Id }, ToDto(exec));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ScheduleExecutionDto>> Update(int id, UpdateScheduleExecutionRequest request)
    {
        var exec = await _db.ScheduleExecutions.Include(s => s.Line).FirstOrDefaultAsync(s => s.Id == id);
        if (exec == null) return NotFound();

        exec.DepartureTime = request.DepartureTime;
        exec.ArrivalTime = request.ArrivalTime;
        exec.Cancelled = request.Cancelled;
        await _db.SaveChangesAsync();
        return ToDto(exec);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var exec = await _db.ScheduleExecutions.FindAsync(id);
        if (exec == null) return NotFound();

        _db.ScheduleExecutions.Remove(exec);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ScheduleExecutionDto ToDto(ScheduleExecution s) =>
        new(s.Id, s.LineId, s.Line.Code, s.Date, s.DepartureTime, s.ArrivalTime, s.Cancelled);
}
