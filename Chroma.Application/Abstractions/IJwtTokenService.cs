namespace Chroma.Application.Abstractions;

public sealed class TokenPair
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAtUtc { get; init; }
    public DateTime RefreshTokenExpiresAtUtc { get; init; }
}

public interface IJwtTokenService
{
    TokenPair GenerateTokens(Guid userId, Guid tenantId, string email, IReadOnlyCollection<string> permissions);
    string HashRefreshToken(string refreshToken);
}
