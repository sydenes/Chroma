using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Notes.Dtos;
using Chroma.Application.Modules.Notes.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/notes")]
public class NotesController(INoteService noteService) : ControllerBase
{
    [RequirePermission("notes.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] NoteSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await noteService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("notes.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var note = await noteService.GetByIdAsync(id, cancellationToken);
        return note is null
            ? NotFound(ApiResponse.Fail("Note not found."))
            : Ok(ApiResponse<NoteDto>.Ok(note));
    }

    [RequirePermission("notes.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateNoteRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(ApiResponse.Fail("Content is required."));

        var note = await noteService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = note.Id }, ApiResponse<NoteDto>.Ok(note));
    }

    [RequirePermission("notes.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateNoteRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest(ApiResponse.Fail("Content is required."));

        var note = await noteService.UpdateAsync(id, request, cancellationToken);
        return note is null
            ? NotFound(ApiResponse.Fail("Note not found."))
            : Ok(ApiResponse<NoteDto>.Ok(note));
    }

    [RequirePermission("notes.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await noteService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Note deleted."))
            : NotFound(ApiResponse.Fail("Note not found."));
    }
}
