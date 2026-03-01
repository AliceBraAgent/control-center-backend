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
    public async Task<IActionResult> GetAll([FromQuery] Guid? parentId)
    {
        var spaces = await spaceService.GetAllAsync(parentId);
        return Ok(spaces);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SpaceDetailDto), StatusCodes.Status200OK)]
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

    [HttpGet("{id:guid}/children")]
    [ProducesResponseType(typeof(IEnumerable<SpaceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChildren(Guid id)
    {
        var children = await spaceService.GetChildrenAsync(id);
        return Ok(children);
    }

    [HttpGet("{id:guid}/path")]
    [ProducesResponseType(typeof(IEnumerable<SpaceBreadcrumbDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPath(Guid id)
    {
        var path = await spaceService.GetPathAsync(id);
        return Ok(path);
    }

    [HttpGet("{id:guid}/relations")]
    [ProducesResponseType(typeof(IEnumerable<SpaceRelationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRelations(Guid id)
    {
        var relations = await spaceService.GetRelationsAsync(id);
        return Ok(relations);
    }

    [HttpPost("{id:guid}/relations")]
    [ProducesResponseType(typeof(SpaceRelationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateRelation(Guid id, [FromBody] CreateSpaceRelationRequest request)
    {
        var relation = await spaceService.CreateRelationAsync(id, request);
        return relation is null ? NotFound() : Created($"/api/spaces/{id}/relations", relation);
    }

    [HttpDelete("{id:guid}/relations/{relationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRelation(Guid id, Guid relationId)
    {
        var deleted = await spaceService.DeleteRelationAsync(id, relationId);
        return deleted ? NoContent() : NotFound();
    }
}
