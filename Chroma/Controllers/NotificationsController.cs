using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Notifications.Dtos;
using Chroma.Application.Modules.Notifications.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [RequirePermission("notifications.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] NotificationSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await notificationService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("notifications.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var notification = await notificationService.GetByIdAsync(id, cancellationToken);
        return notification is null
            ? NotFound(ApiResponse.Fail("Notification not found."))
            : Ok(ApiResponse<NotificationDto>.Ok(notification));
    }

    [RequirePermission("notifications.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(ApiResponse.Fail("Title is required."));

        var notification = await notificationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction("GetById", new { id = notification.Id }, ApiResponse<NotificationDto>.Ok(notification));
    }

    [RequirePermission("notifications.mark_read")]
    [HttpPost("{id:guid}/mark-read")]
    public async Task<IActionResult> MarkAsReadAsync(Guid id, MarkNotificationReadRequest request, CancellationToken cancellationToken)
    {
        var notification = await notificationService.MarkAsReadAsync(id, request, cancellationToken);
        return notification is null
            ? NotFound(ApiResponse.Fail("Notification not found."))
            : Ok(ApiResponse<NotificationDto>.Ok(notification));
    }

    [RequirePermission("notifications.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await notificationService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("Notification deleted."))
            : NotFound(ApiResponse.Fail("Notification not found."));
    }
}
