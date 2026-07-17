using Chroma.Application.Common.Responses;
using Chroma.Application.Modules.Users.Dtos;
using Chroma.Application.Modules.Users.Services;
using Chroma.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chroma.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    [RequirePermission("users.read")]
    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery] UserSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await userService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(response));
    }

    [RequirePermission("users.read")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return user is null
            ? NotFound(ApiResponse.Fail("users.notFound", "User not found."))
            : Ok(ApiResponse<UserDto>.Ok(user));
    }

    [RequirePermission("users.create")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse.Fail(
                "users.createFieldsRequired",
                "First name, last name, email, and password are required."));
        }

        try
        {
            var user = await userService.CreateAsync(request, cancellationToken);
            return CreatedAtAction("GetById", new { id = user.Id }, ApiResponse<UserDto>.Ok(user));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse.Fail(ex.Message));
        }
    }

    [RequirePermission("users.update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest(ApiResponse.Fail("users.nameRequired", "First name and last name are required."));
        }

        try
        {
            var user = await userService.UpdateAsync(id, request, cancellationToken);
            return user is null
                ? NotFound(ApiResponse.Fail("users.notFound", "User not found."))
                : Ok(ApiResponse<UserDto>.Ok(user));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse.Fail(ex.Message));
        }
    }
}
