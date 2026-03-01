using ControlCenter.Api.Models.Dtos;
using ControlCenter.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControlCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await taskService.GetAllAsync();
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var task = await taskService.GetByIdAsync(id);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpGet("by-space/{spaceId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySpace(Guid spaceId)
    {
        var tasks = await taskService.GetBySpaceIdAsync(spaceId);
        return Ok(tasks);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var task = await taskService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request)
    {
        var task = await taskService.UpdateAsync(id, request);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await taskService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
