using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Roles.Dtos;
using Chroma.Application.Modules.Roles.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/roles")]
public class RolesController(IRoleService roleService) : ControllerBase
{
    [RequirePermission("roles.read")]
    [HttpGet]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var roles = await roleService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(roles));
    }

    [RequirePermission("roles.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var role = await roleService.GetByIdAsync(id, cancellationToken);
        return role is null
            ? NotFound(ApiResponse.Fail("roles.notFound", "Role not found."))
            : Ok(ApiResponse<RoleDto>.Ok(role));
    }

    [RequirePermission("roles.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(ApiResponse.Fail("roles.nameRequired", "Name is required."));
        }

        var role = await roleService.CreateAsync(request, cancellationToken);
        return CreatedAtAction("GetById", new { id = role.Id }, ApiResponse<RoleDto>.Ok(role));
    }

    [RequirePermission("roles.manage_permissions")]
    [HttpPut("{id:guid}/permissions")]
    public async Task<IActionResult> UpdatePermissionsAsync(
        Guid id,
        UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var role = await roleService.UpdatePermissionsAsync(id, request, cancellationToken);
        return role is null
            ? NotFound(ApiResponse.Fail("roles.notFound", "Role not found."))
            : Ok(ApiResponse<RoleDto>.Ok(role));
    }
}
