using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Auth.Dtos;
using Chroma.Application.Modules.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TenantSlug) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse.Fail("TenantSlug, Email and Password are required."));
        }

        var result = await authService.LoginAsync(request, cancellationToken);
        return result is null
            ? Unauthorized(ApiResponse.Fail("Invalid tenant, email or password."))
            : Ok(ApiResponse<LoginResult>.Ok(result));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(ApiResponse.Fail("RefreshToken is required."));
        }

        var result = await authService.RefreshAsync(request, cancellationToken);
        return result is null
            ? Unauthorized(ApiResponse.Fail("Invalid or expired refresh token."))
            : Ok(ApiResponse<LoginResult>.Ok(result));
    }

    [Authorize]
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(ApiResponse<AuthStatusDto>.Ok(authService.GetStatus()));
    }
}
