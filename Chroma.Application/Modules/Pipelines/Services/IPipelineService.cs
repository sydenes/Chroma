using Chroma.Application.Modules.Pipelines.Dtos;

namespace Chroma.Application.Modules.Pipelines.Services;

public interface IPipelineService
{
    Task<PipelineSearchResult> SearchAsync(PipelineSearchRequest request, CancellationToken cancellationToken);
    Task<PipelineDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PipelineDto> CreateAsync(CreatePipelineRequest request, CancellationToken cancellationToken);
    Task<PipelineDto?> UpdateAsync(Guid id, UpdatePipelineRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<StageDto> CreateStageAsync(Guid pipelineId, CreateStageRequest request, CancellationToken cancellationToken);
    Task<StageDto?> UpdateStageAsync(Guid pipelineId, Guid stageId, UpdateStageRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteStageAsync(Guid pipelineId, Guid stageId, CancellationToken cancellationToken);
}
