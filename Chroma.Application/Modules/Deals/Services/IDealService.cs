using Chroma.Application.Modules.Deals.Dtos;

namespace Chroma.Application.Modules.Deals.Services;

public interface IDealService
{
    Task<DealSearchResult> SearchAsync(DealSearchRequest request, CancellationToken cancellationToken);
    Task<DealDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<DealBoardDto?> GetBoardAsync(Guid pipelineId, CancellationToken cancellationToken);
    Task<DealDto> CreateAsync(CreateDealRequest request, CancellationToken cancellationToken);
    Task<DealDto?> UpdateAsync(Guid id, UpdateDealRequest request, CancellationToken cancellationToken);
    Task<DealDto?> MoveStageAsync(Guid id, MoveDealStageRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
