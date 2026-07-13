using Chroma.Application.Modules.Roles.Dtos;

namespace Chroma.Application.Modules.Roles.Services;

public interface IRoleService
{
    Task<IReadOnlyCollection<RoleDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<RoleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken);
    Task<RoleDto?> UpdatePermissionsAsync(Guid id, UpdateRolePermissionsRequest request, CancellationToken cancellationToken);
}
