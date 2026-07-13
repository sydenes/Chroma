using Chroma.Application.Modules.Tasks.Dtos;

namespace Chroma.Application.Modules.Tasks.Services;

public interface ITaskService
{
    Task<CrmTaskSearchResult> SearchAsync(CrmTaskSearchRequest request, CancellationToken cancellationToken);
    Task<CrmTaskDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CrmTaskDto> CreateAsync(CreateCrmTaskRequest request, CancellationToken cancellationToken);
    Task<CrmTaskDto?> UpdateAsync(Guid id, UpdateCrmTaskRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
