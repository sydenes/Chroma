using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Messages.Dtos;
using Chroma.Application.Modules.Messages.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/messages")]
public class MessagesController(IMessageService messageService) : ControllerBase
{
    [RequirePermission("messages.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] MessageSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await messageService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("messages.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var message = await messageService.GetByIdAsync(id, cancellationToken);
        return message is null
            ? NotFound(ApiResponse.Fail("messages.notFound", "Message not found."))
            : Ok(ApiResponse<MessageDto>.Ok(message));
    }

    [RequirePermission("messages.send")]
    [HttpPost]
    public async Task<IActionResult> SendOutboundAsync(SendOutboundMessageRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text)
            && string.IsNullOrWhiteSpace(request.MediaUrl)
            && !request.FileId.HasValue)
            return BadRequest(ApiResponse.Fail("messages.textOrMediaUrlRequired", "Text or media URL is required."));

        var message = await messageService.SendOutboundAsync(request, cancellationToken);
        return CreatedAtAction("GetById", new { id = message.Id }, ApiResponse<MessageDto>.Ok(message));
    }
}
