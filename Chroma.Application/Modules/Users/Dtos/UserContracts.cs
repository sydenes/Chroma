namespace Chroma.Application.Modules.Users.Dtos;

public sealed class UserDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Roles { get; init; } = [];
}

public sealed class UserSearchRequest
{
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class UserSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<UserDto> Items { get; init; } = [];
}

public sealed class CreateUserRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public IReadOnlyCollection<Guid> RoleIds { get; init; } = [];
}

public sealed class UpdateUserRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string Status { get; init; } = "active";
    public IReadOnlyCollection<Guid> RoleIds { get; init; } = [];
}
