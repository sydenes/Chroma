using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Users.Dtos;
using Chroma.Application.Modules.Users.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chroma.Infrastructure.Services;

public class UserService(
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant,
    IPasswordHasher passwordHasher) : IUserService
{
    public async Task<UserSearchResult> SearchAsync(UserSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.Users.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var text = request.Query.Trim();
            queryable = queryable.Where(x =>
                (x.FirstName + " " + x.LastName).Contains(text) || x.Email.Contains(text));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var users = await queryable
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userIds = users.Select(x => x.Id).ToArray();
        var rolesByUser = await GetRolesByUserIdsAsync(userIds, cancellationToken);

        return new UserSearchResult
        {
            TotalCount = totalCount,
            Items = users.Select(user => ToDto(user, rolesByUser.GetValueOrDefault(user.Id, []))).ToArray()
        };
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var roles = await GetRoleNamesForUserAsync(user.Id, cancellationToken);
        return ToDto(user, roles);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var email = request.Email.Trim().ToLowerInvariant();
        var exists = await dbContext.Users.AnyAsync(x => x.TenantId == tenantId && x.Email == email, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User
        {
            TenantId = tenantId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Phone = request.Phone,
            PasswordHash = passwordHasher.Hash(request.Password),
            Status = "active"
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        await ReplaceUserRolesAsync(user.Id, request.RoleIds, cancellationToken);

        var roles = await GetRoleNamesForUserAsync(user.Id, cancellationToken);
        return ToDto(user, roles);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Phone = request.Phone;
        user.Status = string.IsNullOrWhiteSpace(request.Status) ? user.Status : request.Status.Trim().ToLowerInvariant();
        user.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await ReplaceUserRolesAsync(user.Id, request.RoleIds, cancellationToken);

        var roles = await GetRoleNamesForUserAsync(user.Id, cancellationToken);
        return ToDto(user, roles);
    }

    private async Task ReplaceUserRolesAsync(Guid userId, IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken)
    {
        var existing = await dbContext.UserRoles.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
        dbContext.UserRoles.RemoveRange(existing);

        foreach (var roleId in roleIds.Distinct())
        {
            dbContext.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<string>> GetRoleNamesForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Join(dbContext.Roles.AsNoTracking(), ur => ur.RoleId, r => r.Id, (_, role) => role.Name)
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<Guid, IReadOnlyCollection<string>>> GetRolesByUserIdsAsync(
        Guid[] userIds,
        CancellationToken cancellationToken)
    {
        var rows = await dbContext.UserRoles
            .AsNoTracking()
            .Where(x => userIds.Contains(x.UserId))
            .Join(dbContext.Roles.AsNoTracking(), ur => ur.RoleId, r => r.Id, (ur, role) => new { ur.UserId, role.Name })
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => (IReadOnlyCollection<string>)g.Select(x => x.Name).ToArray());
    }

    private static UserDto ToDto(User user, IReadOnlyCollection<string> roles)
    {
        return new UserDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Status = user.Status,
            Roles = roles
        };
    }
}
