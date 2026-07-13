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
        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == request.TenantSlug.Trim().ToLowerInvariant(), cancellationToken);

        if (tenant is null)
        {
            return null;
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.TenantId == tenant.Id && x.Email == email, cancellationToken);

        if (user is null || user.Status != "active" || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var permissions = await GetUserPermissionsAsync(user.Id, cancellationToken);
        var tokens = jwtTokenService.GenerateTokens(user.Id, user.TenantId, user.Email, permissions);

        await StoreRefreshTokenAsync(user.Id, tokens, cancellationToken);

        user.LastLoginAtUtc = DateTime.UtcNow;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return BuildLoginResult(user, permissions, tokens);
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

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        storedToken.UpdatedAtUtc = DateTime.UtcNow;

        var permissions = await GetUserPermissionsAsync(user.Id, cancellationToken);
        var tokens = jwtTokenService.GenerateTokens(user.Id, user.TenantId, user.Email, permissions);
        await StoreRefreshTokenAsync(user.Id, tokens, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return BuildLoginResult(user, permissions, tokens);
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

    private async Task StoreRefreshTokenAsync(Guid userId, TokenPair tokens, CancellationToken cancellationToken)
    {
        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = jwtTokenService.HashRefreshToken(tokens.RefreshToken),
            ExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Join(dbContext.RolePermissions.AsNoTracking(), ur => ur.RoleId, rp => rp.RoleId, (_, rp) => rp.PermissionId)
            .Join(dbContext.Permissions.AsNoTracking(), permissionId => permissionId, p => p.Id, (_, p) => p.Key)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private static LoginResult BuildLoginResult(User user, IReadOnlyCollection<string> permissions, TokenPair tokens)
    {
        return new LoginResult
        {
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
                TenantId = user.TenantId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Permissions = permissions
            }
        };
    }
}
