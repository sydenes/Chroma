namespace Chroma.Application.Modules.Roles.Dtos;

public sealed class RoleDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Permissions { get; init; } = [];
}

public sealed class CreateRoleRequest
{
    public string Name { get; init; } = string.Empty;
}

public sealed class UpdateRolePermissionsRequest
{
    public IReadOnlyCollection<string> PermissionKeys { get; init; } = [];
}
