using Chroma.Application.Modules.Auth.Dtos;

namespace Chroma.Application.Modules.Auth.Services;

public interface IAuthService
{
    Task<LoginResult?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<LoginResult?> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    AuthStatusDto GetStatus();
}
