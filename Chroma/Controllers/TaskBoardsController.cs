using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.TaskBoards.Dtos;
using Chroma.Application.Modules.TaskBoards.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/task-boards")]
public class TaskBoardsController(ITaskBoardService taskBoardService) : ControllerBase
{
    [RequirePermission("taskboards.read")]
    [HttpGet("default")]
    public async Task<IActionResult> GetDefaultAsync(CancellationToken cancellationToken)
    {
        var board = await taskBoardService.GetDefaultBoardAsync(cancellationToken);
        return Ok(ApiResponse<TaskBoardDto>.Ok(board));
    }

    [RequirePermission("taskboards.update")]
    [HttpPost("{boardId:guid}/columns")]
    public async Task<IActionResult> CreateColumnAsync(
        Guid boardId,
        CreateTaskColumnRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("taskboards.columnNameRequired", "Column name is required."));

        var column = await taskBoardService.CreateColumnAsync(boardId, request, cancellationToken);
        return Ok(ApiResponse<TaskColumnDto>.Ok(column));
    }

    [RequirePermission("taskboards.update")]
    [HttpPut("columns/{columnId:guid}")]
    public async Task<IActionResult> UpdateColumnAsync(
        Guid columnId,
        UpdateTaskColumnRequest request,
        CancellationToken cancellationToken)
    {
        var column = await taskBoardService.UpdateColumnAsync(columnId, request, cancellationToken);
        return column is null
            ? NotFound(ApiResponse.Fail("taskboards.columnNotFound", "Column not found."))
            : Ok(ApiResponse<TaskColumnDto>.Ok(column));
    }

    [RequirePermission("taskboards.update")]
    [HttpDelete("columns/{columnId:guid}")]
    public async Task<IActionResult> DeleteColumnAsync(Guid columnId, CancellationToken cancellationToken)
    {
        var deleted = await taskBoardService.DeleteColumnAsync(columnId, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("taskboards.columnDeleted", "Column deleted."))
            : NotFound(ApiResponse.Fail("taskboards.columnNotFound", "Column not found."));
    }

    [RequirePermission("taskboards.update")]
    [HttpPost("{boardId:guid}/labels")]
    public async Task<IActionResult> CreateLabelAsync(
        Guid boardId,
        CreateTaskLabelRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse.Fail("taskboards.labelNameRequired", "Label name is required."));

        var label = await taskBoardService.CreateLabelAsync(boardId, request, cancellationToken);
        return Ok(ApiResponse<TaskLabelDto>.Ok(label));
    }

    [RequirePermission("taskboards.update")]
    [HttpPut("labels/{labelId:guid}")]
    public async Task<IActionResult> UpdateLabelAsync(
        Guid labelId,
        UpdateTaskLabelRequest request,
        CancellationToken cancellationToken)
    {
        var label = await taskBoardService.UpdateLabelAsync(labelId, request, cancellationToken);
        return label is null
            ? NotFound(ApiResponse.Fail("taskboards.labelNotFound", "Label not found."))
            : Ok(ApiResponse<TaskLabelDto>.Ok(label));
    }

    [RequirePermission("taskboards.update")]
    [HttpDelete("labels/{labelId:guid}")]
    public async Task<IActionResult> DeleteLabelAsync(Guid labelId, CancellationToken cancellationToken)
    {
        var deleted = await taskBoardService.DeleteLabelAsync(labelId, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("taskboards.labelDeleted", "Label deleted."))
            : NotFound(ApiResponse.Fail("taskboards.labelNotFound", "Label not found."));
    }
}
