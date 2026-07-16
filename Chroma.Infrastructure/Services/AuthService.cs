using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Auth.Dtos;
using Chroma.Application.Modules.Auth.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chroma.Infrastructure.Services;

public class AuthService(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    ICurrentUser currentUser) : IAuthService
{
    public async Task<LoginResult?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (user is null || user.Status != "active" || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var memberships = await GetActiveTenantMembershipsAsync(user.Id, cancellationToken);
        if (memberships.Count == 0)
        {
            return null;
        }

        if (request.TenantId is null && memberships.Count > 1)
        {
            return new LoginResult
            {
                RequiresTenantSelection = true,
                AvailableTenants = memberships.Select(ToTenantOption).ToArray()
            };
        }

        var membership = request.TenantId is null
            ? memberships[0]
            : memberships.FirstOrDefault(x => x.TenantId == request.TenantId.Value);

        if (membership is null)
        {
            return null;
        }

        var permissions = await GetUserPermissionsAsync(user.Id, membership.TenantId, cancellationToken);
        var tokens = jwtTokenService.GenerateTokens(user.Id, membership.TenantId, user.Email, permissions);

        await StoreRefreshTokenAsync(user.Id, membership.TenantId, tokens, cancellationToken);

        user.LastLoginAtUtc = DateTime.UtcNow;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return BuildLoginResult(user, membership, permissions, tokens);
    }

    public async Task<LoginResult?> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = jwtTokenService.HashRefreshToken(request.RefreshToken);
        var storedToken = await dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || storedToken.RevokedAtUtc is not null || storedToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return null;
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == storedToken.UserId, cancellationToken);
        if (user is null || user.Status != "active")
        {
            return null;
        }

        var membership = await GetActiveTenantMembershipAsync(user.Id, storedToken.TenantId, cancellationToken);
        if (membership is null)
        {
            return null;
        }

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        storedToken.UpdatedAtUtc = DateTime.UtcNow;

        var permissions = await GetUserPermissionsAsync(user.Id, membership.TenantId, cancellationToken);
        var tokens = jwtTokenService.GenerateTokens(user.Id, membership.TenantId, user.Email, permissions);
        await StoreRefreshTokenAsync(user.Id, membership.TenantId, tokens, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return BuildLoginResult(user, membership, permissions, tokens);
    }

    public AuthStatusDto GetStatus()
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null || currentUser.TenantId is null)
        {
            return new AuthStatusDto { Authenticated = false };
        }

        return new AuthStatusDto
        {
            Authenticated = true,
            User = new AuthUserDto
            {
                Id = currentUser.UserId.Value,
                TenantId = currentUser.TenantId.Value,
                Email = currentUser.Email ?? string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                Permissions = currentUser.Permissions
            }
        };
    }

    private async Task StoreRefreshTokenAsync(Guid userId, Guid tenantId, TokenPair tokens, CancellationToken cancellationToken)
    {
        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            TenantId = tenantId,
            TokenHash = jwtTokenService.HashRefreshToken(tokens.RefreshToken),
            ExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<string>> GetUserPermissionsAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        return await dbContext.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Join(
                dbContext.Roles.AsNoTracking().Where(role => role.TenantId == tenantId),
                userRole => userRole.RoleId,
                role => role.Id,
                (_, role) => role.Id)
            .Join(dbContext.RolePermissions.AsNoTracking(), roleId => roleId, rp => rp.RoleId, (_, rp) => rp.PermissionId)
            .Join(dbContext.Permissions.AsNoTracking(), permissionId => permissionId, p => p.Id, (_, p) => p.Key)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<List<UserTenant>> GetActiveTenantMembershipsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.UserTenants
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Status == "active" && x.Tenant.Status == "active")
            .Include(x => x.Tenant)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Tenant.Name)
            .ToListAsync(cancellationToken);
    }

    private async Task<UserTenant?> GetActiveTenantMembershipAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        return await dbContext.UserTenants
            .AsNoTracking()
            .Include(x => x.Tenant)
            .FirstOrDefaultAsync(
                x => x.UserId == userId &&
                    x.TenantId == tenantId &&
                    x.Status == "active" &&
                    x.Tenant.Status == "active",
                cancellationToken);
    }

    private static AuthTenantOptionDto ToTenantOption(UserTenant membership)
    {
        return new AuthTenantOptionDto
        {
            TenantId = membership.TenantId,
            TenantName = membership.Tenant.Name,
            TenantSlug = membership.Tenant.Slug,
            Logo = membership.Tenant.Logo,
            IsDefault = membership.IsDefault
        };
    }

    private static LoginResult BuildLoginResult(
        User user,
        UserTenant membership,
        IReadOnlyCollection<string> permissions,
        TokenPair tokens)
    {
        return new LoginResult
        {
            RequiresTenantSelection = false,
            Tokens = new AuthTokensDto
            {
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                AccessTokenExpiresAtUtc = tokens.AccessTokenExpiresAtUtc,
                RefreshTokenExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc
            },
            User = new AuthUserDto
            {
                Id = user.Id,
                TenantId = membership.TenantId,
                TenantName = membership.Tenant.Name,
                TenantSlug = membership.Tenant.Slug,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Permissions = permissions
            }
        };
    }
}
