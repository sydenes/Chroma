using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Subscriptions.Dtos;
using Chroma.Application.Modules.Subscriptions.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/subscriptions")]
public class SubscriptionsController(ISubscriptionService subscriptionService) : ControllerBase
{
    [RequirePermission("subscriptions.read")]
    [HttpGet("plans")]
    public async Task<IActionResult> ListPlansAsync(CancellationToken cancellationToken)
    {
        var plans = await subscriptionService.ListPlansAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<SubscriptionPlanDto>>.Ok(plans));
    }

    [RequirePermission("subscriptions.manage")]
    [HttpPost("plans")]
    public async Task<IActionResult> CreatePlanAsync(
        CreateSubscriptionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var plan = await subscriptionService.CreatePlanAsync(request, cancellationToken);
        return Ok(ApiResponse<SubscriptionPlanDto>.Ok(plan));
    }

    [RequirePermission("subscriptions.manage")]
    [HttpPut("plans/{id:guid}")]
    public async Task<IActionResult> UpdatePlanAsync(
        Guid id,
        UpdateSubscriptionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var plan = await subscriptionService.UpdatePlanAsync(id, request, cancellationToken);
        return plan is null
            ? NotFound(ApiResponse.Fail("subscriptions.planNotFound", "Subscription plan not found."))
            : Ok(ApiResponse<SubscriptionPlanDto>.Ok(plan));
    }

    [RequirePermission("subscriptions.manage")]
    [HttpDelete("plans/{id:guid}")]
    public async Task<IActionResult> DeletePlanAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await subscriptionService.DeletePlanAsync(id, cancellationToken);
        return deleted
            ? Ok(ApiResponse.Ok("subscriptions.planDeleted", "Subscription plan deleted."))
            : NotFound(ApiResponse.Fail("subscriptions.planNotFound", "Subscription plan not found."));
    }

    [RequirePermission("subscriptions.read")]
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var subscription = await subscriptionService.GetCurrentAsync(cancellationToken);
        return Ok(ApiResponse<TenantSubscriptionDto>.Ok(subscription));
    }

    [RequirePermission("subscriptions.manage")]
    [HttpPut("current")]
    public async Task<IActionResult> AssignCurrentAsync(
        AssignTenantSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var subscription = await subscriptionService.AssignCurrentAsync(request, cancellationToken);
        return Ok(ApiResponse<TenantSubscriptionDto>.Ok(subscription));
    }
}
