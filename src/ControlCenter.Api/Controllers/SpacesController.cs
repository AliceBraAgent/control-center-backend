using ControlCenter.Api.Models.Dtos;
using ControlCenter.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControlCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SpacesController(ISpaceService spaceService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SpaceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var spaces = await spaceService.GetAllAsync();
        return Ok(spaces);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SpaceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var space = await spaceService.GetByIdAsync(id);
        return space is null ? NotFound() : Ok(space);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SpaceDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSpaceRequest request)
    {
        var space = await spaceService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = space.Id }, space);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SpaceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSpaceRequest request)
    {
        var space = await spaceService.UpdateAsync(id, request);
        return space is null ? NotFound() : Ok(space);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await spaceService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
