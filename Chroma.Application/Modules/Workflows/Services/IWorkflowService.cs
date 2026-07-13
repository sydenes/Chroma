using Chroma.Application.Modules.Workflows.Dtos;

namespace Chroma.Application.Modules.Workflows.Services;

public interface IWorkflowService
{
    Task<WorkflowSearchResult> SearchAsync(WorkflowSearchRequest request, CancellationToken cancellationToken);
    Task<WorkflowDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<WorkflowDto> CreateAsync(CreateWorkflowRequest request, CancellationToken cancellationToken);
    Task<WorkflowDto?> UpdateAsync(Guid id, UpdateWorkflowRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
