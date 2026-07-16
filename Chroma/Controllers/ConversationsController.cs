using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Conversations.Dtos;
using Chroma.Application.Modules.Conversations.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/conversations")]
public class ConversationsController(IConversationService conversationService) : ControllerBase
{
    [RequirePermission("conversations.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] ConversationSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await conversationService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("conversations.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var conversation = await conversationService.GetByIdAsync(id, cancellationToken);
        return conversation is null
            ? NotFound(ApiResponse.Fail("Conversation not found."))
            : Ok(ApiResponse<ConversationDto>.Ok(conversation));
    }

    [RequirePermission("conversations.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateConversationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var conversation = await conversationService.CreateAsync(request, cancellationToken);
            return CreatedAtAction("GetById", new { id = conversation.Id }, ApiResponse<ConversationDto>.Ok(conversation));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse.Fail(ex.Message));
        }
    }

    [RequirePermission("conversations.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateConversationRequest request, CancellationToken cancellationToken)
    {
        var conversation = await conversationService.UpdateAsync(id, request, cancellationToken);
        return conversation is null
            ? NotFound(ApiResponse.Fail("Conversation not found."))
            : Ok(ApiResponse<ConversationDto>.Ok(conversation));
    }

    [RequirePermission("conversations.assign")]
    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> AssignAsync(Guid id, AssignConversationRequest request, CancellationToken cancellationToken)
    {
        var conversation = await conversationService.AssignAsync(id, request, cancellationToken);
        return conversation is null
            ? NotFound(ApiResponse.Fail("Conversation not found."))
            : Ok(ApiResponse<ConversationDto>.Ok(conversation));
    }

    [RequirePermission("conversations.update_status")]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatusAsync(Guid id, UpdateConversationStatusRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
            return BadRequest(ApiResponse.Fail("Status is required."));

        var conversation = await conversationService.UpdateStatusAsync(id, request, cancellationToken);
        return conversation is null
            ? NotFound(ApiResponse.Fail("Conversation not found."))
            : Ok(ApiResponse<ConversationDto>.Ok(conversation));
    }

    [RequirePermission("conversations.mark_read")]
    [HttpPost("{id:guid}/mark-read")]
    public async Task<IActionResult> MarkAsReadAsync(Guid id, CancellationToken cancellationToken)
    {
        var conversation = await conversationService.MarkAsReadAsync(id, cancellationToken);
        return conversation is null
            ? NotFound(ApiResponse.Fail("Conversation not found."))
            : Ok(ApiResponse<ConversationDto>.Ok(conversation));
    }

    [RequirePermission("conversations.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await conversationService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Conversation deleted."))
            : NotFound(ApiResponse.Fail("Conversation not found."));
    }
}
