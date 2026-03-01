using ControlCenter.Api.Models.Dtos;
using ControlCenter.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ControlCenter.Api.Controllers;

[ApiController]
[Route("api/spaces/{spaceId:guid}/documents")]
[Produces("application/json")]
public class DocumentsController(IDocumentService documentService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DocumentListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(Guid spaceId)
    {
        var docs = await documentService.GetAllBySpaceAsync(spaceId);
        return Ok(docs);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid spaceId, Guid id)
    {
        var doc = await documentService.GetByIdAsync(spaceId, id);
        return doc is null ? NotFound() : Ok(doc);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(Guid spaceId, [FromBody] CreateDocumentRequest request)
    {
        var doc = await documentService.CreateAsync(spaceId, request);
        return doc is null
            ? NotFound()
            : CreatedAtAction(nameof(GetById), new { spaceId, id = doc.Id }, doc);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid spaceId, Guid id, [FromBody] UpdateDocumentRequest request)
    {
        var doc = await documentService.UpdateAsync(spaceId, id, request);
        return doc is null ? NotFound() : Ok(doc);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid spaceId, Guid id)
    {
        var deleted = await documentService.DeleteAsync(spaceId, id);
        return deleted ? NoContent() : NotFound();
    }
}
