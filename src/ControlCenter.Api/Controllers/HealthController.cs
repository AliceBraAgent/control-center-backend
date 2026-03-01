using Microsoft.AspNetCore.Mvc;

namespace ControlCenter.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet("/health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            service = "BRABENDER Control Center API",
            timestamp = DateTime.UtcNow
        });
    }
}
