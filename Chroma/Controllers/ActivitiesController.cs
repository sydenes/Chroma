using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Activities.Dtos;
using Chroma.Application.Modules.Activities.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/activities")]
public class ActivitiesController(IActivityService activityService) : ControllerBase
{
    [RequirePermission("activities.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] ActivitySearchRequest request, CancellationToken cancellationToken)
    {
        var response = await activityService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("activities.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var activity = await activityService.GetByIdAsync(id, cancellationToken);
        return activity is null
            ? NotFound(ApiResponse.Fail("activities.notFound", "Activity not found."))
            : Ok(ApiResponse<ActivityDto>.Ok(activity));
    }

    [RequirePermission("activities.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateActivityRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Subject))
            return BadRequest(ApiResponse.Fail("activities.subjectRequired", "Subject is required."));

        var activity = await activityService.CreateAsync(request, cancellationToken);
        return CreatedAtAction("GetById", new { id = activity.Id }, ApiResponse<ActivityDto>.Ok(activity));
    }

    [RequirePermission("activities.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateActivityRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Subject))
            return BadRequest(ApiResponse.Fail("activities.subjectRequired", "Subject is required."));

        var activity = await activityService.UpdateAsync(id, request, cancellationToken);
        return activity is null
            ? NotFound(ApiResponse.Fail("activities.notFound", "Activity not found."))
            : Ok(ApiResponse<ActivityDto>.Ok(activity));
    }

    [RequirePermission("activities.delete")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await activityService.DeleteAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("activities.deleted", "Activity deleted."))
            : NotFound(ApiResponse.Fail("activities.notFound", "Activity not found."));
    }
}
