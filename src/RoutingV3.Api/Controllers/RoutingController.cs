using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoutingV3.Engine;
using RoutingV3.Infrastructure;

namespace RoutingV3.Api.Controllers;

[ApiController]
[Route("api/v3/routing")]
public class RoutingController : ControllerBase
{
    private readonly RoutingDbContext _db;
    private readonly RoutingEngine _routingEngine;

    public RoutingController(RoutingDbContext db, RoutingEngine routingEngine)
    {
        _db = db;
        _routingEngine = routingEngine;
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<RoutingResult>> Calculate(ShipmentRequest request)
    {
        // Load all data and rebuild the graph
        var lines = await _db.Lines
            .Include(l => l.OriginHub)
            .Include(l => l.DestinationHub)
            .Include(l => l.OriginPostalCodeArea)
            .Include(l => l.DestinationPostalCodeArea)
            .Include(l => l.LineMandates).ThenInclude(lm => lm.Mandate)
            .Include(l => l.ScheduleRules)
            .ToListAsync();

        var postalCodeAreas = await _db.PostalCodeAreas.ToListAsync();

        _routingEngine.LoadData(lines, postalCodeAreas);

        var result = _routingEngine.Calculate(request);
        return Ok(result);
    }
}
