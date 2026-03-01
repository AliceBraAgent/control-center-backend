using ControlCenter.Api.Models.Dtos;
using ControlCenter.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControlCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SearchController(ISearchService searchService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(SearchResultsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { error = "Search query 'q' is required." });

        var results = await searchService.SearchAsync(q);
        return Ok(results);
    }
}
