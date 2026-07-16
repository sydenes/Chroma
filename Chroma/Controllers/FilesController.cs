using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Files.Dtos;
using Chroma.Application.Modules.Files.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/files")]
public class FilesController(IFileService fileService) : ControllerBase
{
    [RequirePermission("files.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] FileSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await fileService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("files.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var file = await fileService.GetByIdAsync(id, cancellationToken);
        return file is null
            ? NotFound(ApiResponse.Fail("File not found."))
            : Ok(ApiResponse<FileDto>.Ok(file));
    }

    [RequirePermission("files.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateFileRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.Url))
            return BadRequest(ApiResponse.Fail("FileName and Url are required."));

        var file = await fileService.CreateAsync(request, cancellationToken);
        return CreatedAtAction("GetById", new { id = file.Id }, ApiResponse<FileDto>.Ok(file));
    }

    [RequirePermission("files.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateFileRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
            return BadRequest(ApiResponse.Fail("FileName is required."));

        var file = await fileService.UpdateAsync(id, request, cancellationToken);
        return file is null
            ? NotFound(ApiResponse.Fail("File not found."))
            : Ok(ApiResponse<FileDto>.Ok(file));
    }

    [RequirePermission("files.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await fileService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("File deleted."))
            : NotFound(ApiResponse.Fail("File not found."));
    }
}
