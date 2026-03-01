using ControlCenter.Api.Models.Dtos;
using ControlCenter.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControlCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var stats = await dashboardService.GetStatsAsync();
        return Ok(stats);
    }
}
