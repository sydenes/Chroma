namespace Chroma.Application.Modules.Auth.Dtos;

public sealed class LoginRequest
{
    public string TenantSlug { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}

public sealed class AuthTokensDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAtUtc { get; init; }
    public DateTime RefreshTokenExpiresAtUtc { get; init; }
}

public sealed class AuthUserDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Permissions { get; init; } = [];
}

public sealed class LoginResult
{
    public AuthTokensDto Tokens { get; init; } = null!;
    public AuthUserDto User { get; init; } = null!;
}

public sealed class AuthStatusDto
{
    public bool Authenticated { get; init; }
    public AuthUserDto? User { get; init; }
}
