using Chroma.Application.Modules.Activities.Dtos;

namespace Chroma.Application.Modules.Activities.Services;

public interface IActivityService
{
    Task<ActivitySearchResult> SearchAsync(ActivitySearchRequest request, CancellationToken cancellationToken);
    Task<ActivityDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ActivityDto> CreateAsync(CreateActivityRequest request, CancellationToken cancellationToken);
    Task<ActivityDto?> UpdateAsync(Guid id, UpdateActivityRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
