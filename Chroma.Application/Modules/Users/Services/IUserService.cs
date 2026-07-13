using Chroma.Application.Modules.Users.Dtos;

namespace Chroma.Application.Modules.Users.Services;

public interface IUserService
{
    Task<UserSearchResult> SearchAsync(UserSearchRequest request, CancellationToken cancellationToken);
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken);
}
