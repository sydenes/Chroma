using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Roles.Dtos;
using Chroma.Application.Modules.Roles.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chroma.Infrastructure.Services;

public class RoleService(
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant) : IRoleService
{
    public async Task<IReadOnlyCollection<RoleDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var roles = await dbContext.Roles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var result = new List<RoleDto>();
        foreach (var role in roles)
        {
            var permissions = await GetPermissionsForRoleAsync(role.Id, cancellationToken);
            result.Add(ToDto(role, permissions));
        }

        return result;
    }

    public async Task<RoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var role = await dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);

        if (role is null)
        {
            return null;
        }

        var permissions = await GetPermissionsForRoleAsync(role.Id, cancellationToken);
        return ToDto(role, permissions);
    }

    public async Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var role = new Role
        {
            TenantId = tenantId,
            Name = request.Name.Trim()
        };

        dbContext.Roles.Add(role);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(role, []);
    }

    public async Task<RoleDto?> UpdatePermissionsAsync(Guid id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var role = await dbContext.Roles.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (role is null)
        {
            return null;
        }

        var permissionKeys = request.PermissionKeys.Select(x => x.Trim()).Distinct().ToArray();
        var permissions = await dbContext.Permissions
            .Where(x => permissionKeys.Contains(x.Key))
            .ToListAsync(cancellationToken);

        var existing = await dbContext.RolePermissions.Where(x => x.RoleId == role.Id).ToListAsync(cancellationToken);
        dbContext.RolePermissions.RemoveRange(existing);

        foreach (var permission in permissions)
        {
            dbContext.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            });
        }

        role.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(role, permissions.Select(x => x.Key).ToArray());
    }

    private async Task<IReadOnlyCollection<string>> GetPermissionsForRoleAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return await dbContext.RolePermissions
            .AsNoTracking()
            .Where(x => x.RoleId == roleId)
            .Join(dbContext.Permissions.AsNoTracking(), rp => rp.PermissionId, p => p.Id, (_, p) => p.Key)
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }

    private static RoleDto ToDto(Role role, IReadOnlyCollection<string> permissions)
    {
        return new RoleDto
        {
            Id = role.Id,
            TenantId = role.TenantId,
            Name = role.Name,
            Permissions = permissions
        };
    }
}
