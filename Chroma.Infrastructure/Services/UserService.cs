using Chroma.Application.Abstractions;
using Chroma.Application.Common.Exceptions;
using Chroma.Application.Modules.Subscriptions.Services;
using Chroma.Application.Modules.Users.Dtos;
using Chroma.Application.Modules.Users.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chroma.Infrastructure.Services;

public class UserService(
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant,
    IPasswordHasher passwordHasher,
    ISubscriptionService subscriptionService) : IUserService
{
    public async Task<UserSearchResult> SearchAsync(UserSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.UserTenants
            .AsNoTracking()
            .Include(x => x.User)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var text = request.Query.Trim();
            queryable = queryable.Where(x =>
                (x.User.FirstName + " " + x.User.LastName).Contains(text) || x.User.Email.Contains(text));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var memberships = await queryable
            .OrderBy(x => x.User.LastName)
            .ThenBy(x => x.User.FirstName)
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userIds = memberships.Select(x => x.UserId).ToArray();
        var rolesByUser = await GetRolesByUserIdsAsync(userIds, tenantId, cancellationToken);

        return new UserSearchResult
        {
            TotalCount = totalCount,
            Items = memberships
                .Select(membership =>
                {
                    var roles = rolesByUser.GetValueOrDefault(membership.UserId, []);
                    return ToDto(membership, roles);
                })
                .ToArray()
        };
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var membership = await dbContext.UserTenants
            .AsNoTracking()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == id && x.TenantId == tenantId, cancellationToken);

        if (membership is null)
        {
            return null;
        }

        var roles = await GetRolesForUserAsync(membership.UserId, tenantId, cancellationToken);
        return ToDto(membership, roles);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        await subscriptionService.EnsureSeatAvailableAsync(tenantId, cancellationToken);

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (user is not null)
        {
            var membershipExists = await dbContext.UserTenants
                .AnyAsync(x => x.UserId == user.Id && x.TenantId == tenantId, cancellationToken);

            if (membershipExists)
            {
                throw new AppException(
                    "users.emailAlreadyExists",
                    "A user with this email already exists in this workspace.");
            }
        }

        if (user is null)
        {
            user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                Phone = request.Phone,
                PasswordHash = passwordHasher.Hash(request.Password),
                Status = "active"
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var membership = new UserTenant
        {
            UserId = user.Id,
            TenantId = tenantId,
            Status = "active",
            IsDefault = !await dbContext.UserTenants.AnyAsync(x => x.UserId == user.Id, cancellationToken)
        };

        dbContext.UserTenants.Add(membership);
        await dbContext.SaveChangesAsync(cancellationToken);
        await ReplaceUserRolesAsync(user.Id, tenantId, request.RoleIds, cancellationToken);

        membership.User = user;
        var roles = await GetRolesForUserAsync(user.Id, tenantId, cancellationToken);
        return ToDto(membership, roles);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var membership = await dbContext.UserTenants
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.UserId == id && x.TenantId == tenantId, cancellationToken);
        if (membership is null)
        {
            return null;
        }

        var nextStatus = string.IsNullOrWhiteSpace(request.Status)
            ? membership.Status
            : request.Status.Trim().ToLowerInvariant();

        if (!string.Equals(membership.Status, "active", StringComparison.OrdinalIgnoreCase)
            && string.Equals(nextStatus, "active", StringComparison.OrdinalIgnoreCase))
        {
            await subscriptionService.EnsureSeatAvailableAsync(tenantId, cancellationToken);
        }

        var user = membership.User;
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Phone = request.Phone;
        membership.Status = nextStatus;
        user.UpdatedAtUtc = DateTime.UtcNow;
        membership.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await ReplaceUserRolesAsync(user.Id, tenantId, request.RoleIds, cancellationToken);

        var roles = await GetRolesForUserAsync(user.Id, tenantId, cancellationToken);
        return ToDto(membership, roles);
    }

    private async Task ReplaceUserRolesAsync(
        Guid userId,
        Guid tenantId,
        IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken)
    {
        var tenantRoleIds = await dbContext.Roles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var requestedRoleIds = roleIds.Distinct().ToArray();
        if (requestedRoleIds.Any(roleId => !tenantRoleIds.Contains(roleId)))
        {
            throw new AppException(
                "users.rolesNotInWorkspace",
                "The selected roles do not belong to this workspace.");
        }

        var existing = await dbContext.UserRoles
            .Where(x => x.UserId == userId && tenantRoleIds.Contains(x.RoleId))
            .ToListAsync(cancellationToken);
        dbContext.UserRoles.RemoveRange(existing);

        foreach (var roleId in requestedRoleIds)
        {
            dbContext.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<(Guid Id, string Name)>> GetRolesForUserAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var rows = await dbContext.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Join(
                dbContext.Roles.AsNoTracking().Where(role => role.TenantId == tenantId),
                ur => ur.RoleId,
                role => role.Id,
                (_, role) => new { role.Id, role.Name })
            .ToListAsync(cancellationToken);

        return rows.Select(x => (x.Id, x.Name)).ToArray();
    }

    private async Task<Dictionary<Guid, IReadOnlyCollection<(Guid Id, string Name)>>> GetRolesByUserIdsAsync(
        Guid[] userIds,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var rows = await dbContext.UserRoles
            .AsNoTracking()
            .Where(x => userIds.Contains(x.UserId))
            .Join(
                dbContext.Roles.AsNoTracking().Where(role => role.TenantId == tenantId),
                ur => ur.RoleId,
                role => role.Id,
                (ur, role) => new { ur.UserId, role.Id, role.Name })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.UserId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyCollection<(Guid Id, string Name)>)g.Select(x => (x.Id, x.Name)).ToArray());
    }

    private static UserDto ToDto(
        UserTenant membership,
        IReadOnlyCollection<(Guid Id, string Name)> roles)
    {
        return new UserDto
        {
            Id = membership.User.Id,
            TenantId = membership.TenantId,
            FirstName = membership.User.FirstName,
            LastName = membership.User.LastName,
            Email = membership.User.Email,
            Phone = membership.User.Phone,
            Status = membership.Status,
            Roles = roles.Select(x => x.Name).ToArray(),
            RoleIds = roles.Select(x => x.Id).ToArray()
        };
    }
}
