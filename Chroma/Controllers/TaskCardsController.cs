using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.TaskBoards.Dtos;
using Chroma.Application.Modules.TaskBoards.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/task-cards")]
public class TaskCardsController(ITaskBoardService taskBoardService) : ControllerBase
{
    [RequirePermission("taskboards.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var card = await taskBoardService.GetCardByIdAsync(id, cancellationToken);
        return card is null
            ? NotFound(ApiResponse.Fail("taskboards.cardNotFound", "Card not found."))
            : Ok(ApiResponse<TaskCardDetailDto>.Ok(card));
    }

    [RequirePermission("taskboards.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateTaskCardRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(ApiResponse.Fail("taskboards.cardTitleRequired", "Card title is required."));

        var card = await taskBoardService.CreateCardAsync(request, cancellationToken);
        return CreatedAtAction("GetById", new { id = card.Id }, ApiResponse<TaskCardDetailDto>.Ok(card));
    }

    [RequirePermission("taskboards.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        UpdateTaskCardRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(ApiResponse.Fail("taskboards.cardTitleRequired", "Card title is required."));

        var card = await taskBoardService.UpdateCardAsync(id, request, cancellationToken);
        return card is null
            ? NotFound(ApiResponse.Fail("taskboards.cardNotFound", "Card not found."))
            : Ok(ApiResponse<TaskCardDetailDto>.Ok(card));
    }

    [RequirePermission("taskboards.update")]
    [HttpPost("{id:guid}/move")]
    public async Task<IActionResult> MoveAsync(
        Guid id,
        MoveTaskCardRequest request,
        CancellationToken cancellationToken)
    {
        var card = await taskBoardService.MoveCardAsync(id, request, cancellationToken);
        return card is null
            ? NotFound(ApiResponse.Fail("taskboards.cardNotFound", "Card not found."))
            : Ok(ApiResponse<TaskCardSummaryDto>.Ok(card));
    }

    [RequirePermission("taskboards.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await taskBoardService.DeleteCardAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("taskboards.cardDeleted", "Card deleted."))
            : NotFound(ApiResponse.Fail("taskboards.cardNotFound", "Card not found."));
    }

    [RequirePermission("taskboards.update")]
    [HttpPost("{id:guid}/comments")]
    public async Task<IActionResult> AddCommentAsync(
        Guid id,
        CreateTaskCommentRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
            return BadRequest(ApiResponse.Fail("taskboards.commentRequired", "Comment is required."));

        var comment = await taskBoardService.AddCommentAsync(id, request, cancellationToken);
        return Ok(ApiResponse<TaskCommentDto>.Ok(comment));
    }

    [RequirePermission("taskboards.update")]
    [HttpDelete("comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteCommentAsync(Guid commentId, CancellationToken cancellationToken)
    {
        var deleted = await taskBoardService.DeleteCommentAsync(commentId, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("taskboards.commentDeleted", "Comment deleted."))
            : NotFound(ApiResponse.Fail("taskboards.commentNotFound", "Comment not found."));
    }
}
