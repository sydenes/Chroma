using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Tags.Dtos;
using Chroma.Application.Modules.Tags.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/tags")]
public class TagsController(ITagService tagService) : ControllerBase
{
    [RequirePermission("tags.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] TagSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await tagService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("tags.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tag = await tagService.GetByIdAsync(id, cancellationToken);
        return tag is null
            ? NotFound(ApiResponse.Fail("Tag not found."))
            : Ok(ApiResponse<TagDto>.Ok(tag));
    }

    [RequirePermission("tags.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateTagRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("Name is required."));

        var tag = await tagService.CreateAsync(request, cancellationToken);
        return CreatedAtAction("GetById", new { id = tag.Id }, ApiResponse<TagDto>.Ok(tag));
    }

    [RequirePermission("tags.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("Name is required."));

        var tag = await tagService.UpdateAsync(id, request, cancellationToken);
        return tag is null
            ? NotFound(ApiResponse.Fail("Tag not found."))
            : Ok(ApiResponse<TagDto>.Ok(tag));
    }

    [RequirePermission("tags.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await tagService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Tag deleted."))
            : NotFound(ApiResponse.Fail("Tag not found."));
    }
}
