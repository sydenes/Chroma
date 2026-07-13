using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Tenants.Dtos;
using Chroma.Application.Modules.Tenants.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/tenant")]
public class TenantController(ITenantService tenantService) : ControllerBase
{
    [RequirePermission("tenant.settings.read")]
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await tenantService.GetSettingsAsync(cancellationToken);
        return settings is null
            ? NotFound(ApiResponse.Fail("Tenant settings not found."))
            : Ok(ApiResponse<TenantSettingsDto>.Ok(settings));
    }

    [RequirePermission("tenant.settings.update")]
    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettingsAsync(UpdateTenantSettingsRequest request, CancellationToken cancellationToken)
    {
        var settings = await tenantService.UpdateSettingsAsync(request, cancellationToken);
        return settings is null
            ? NotFound(ApiResponse.Fail("Tenant settings not found."))
            : Ok(ApiResponse<TenantSettingsDto>.Ok(settings));
    }
}
