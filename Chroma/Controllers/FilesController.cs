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
            ? NotFound(ApiResponse.Fail("files.notFound", "File not found."))
            : Ok(ApiResponse<FileDto>.Ok(file));
    }

    [RequirePermission("files.read")]
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> DownloadAsync(Guid id, CancellationToken cancellationToken)
    {
        var download = await fileService.OpenDownloadAsync(id, cancellationToken);
        if (download is null)
        {
            return NotFound(ApiResponse.Fail("files.notFound", "File not found."));
        }

        return File(download.Stream, download.ContentType, download.FileName);
    }

    [RequirePermission("files.create")]
    [HttpPost("upload")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 20 * 1024 * 1024)]
    public async Task<IActionResult> UploadAsync(
        IFormFile? file,
        [FromForm] string ownerType,
        [FromForm] Guid ownerId,
        [FromForm] string? category,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length <= 0)
        {
            return BadRequest(ApiResponse.Fail("files.fileRequired", "A valid file is required."));
        }

        if (string.IsNullOrWhiteSpace(ownerType) || ownerId == Guid.Empty)
        {
            return BadRequest(ApiResponse.Fail("files.ownerRequired", "Owner type and owner id are required."));
        }

        await using var stream = file.OpenReadStream();
        var uploaded = await fileService.UploadAsync(
            new UploadFileRequest
            {
                OwnerType = ownerType,
                OwnerId = ownerId,
                Category = category,
                FileName = file.FileName,
                ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType,
                SizeBytes = file.Length,
                Content = stream
            },
            cancellationToken);

        return CreatedAtAction("GetById", new { id = uploaded.Id }, ApiResponse<FileDto>.Ok(uploaded));
    }

    [RequirePermission("files.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateFileRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.OwnerType))
        {
            return BadRequest(ApiResponse.Fail(
                "files.fileNameAndOwnerRequired",
                "File name and owner are required."));
        }

        var created = await fileService.CreateAsync(request, cancellationToken);
        return CreatedAtAction("GetById", new { id = created.Id }, ApiResponse<FileDto>.Ok(created));
    }

    [RequirePermission("files.create")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateFileRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            return BadRequest(ApiResponse.Fail("files.fileNameRequired", "File name is required."));
        }

        var file = await fileService.UpdateAsync(id, request, cancellationToken);
        return file is null
            ? NotFound(ApiResponse.Fail("files.notFound", "File not found."))
            : Ok(ApiResponse<FileDto>.Ok(file));
    }

    [RequirePermission("files.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await fileService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("files.deleted", "File deleted."))
            : NotFound(ApiResponse.Fail("files.notFound", "File not found."));
    }
}
