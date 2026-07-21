using Chroma.Application.Modules.Subscriptions.Dtos;

namespace Chroma.Application.Modules.Subscriptions.Services;

public interface ISubscriptionService
{
    Task<IReadOnlyCollection<SubscriptionPlanDto>> ListPlansAsync(CancellationToken cancellationToken);
    Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanRequest request, CancellationToken cancellationToken);
    Task<SubscriptionPlanDto?> UpdatePlanAsync(Guid id, UpdateSubscriptionPlanRequest request, CancellationToken cancellationToken);
    Task<bool> DeletePlanAsync(Guid id, CancellationToken cancellationToken);
    Task<TenantSubscriptionDto> GetCurrentAsync(CancellationToken cancellationToken);
    Task<TenantSubscriptionDto> AssignCurrentAsync(AssignTenantSubscriptionRequest request, CancellationToken cancellationToken);
    Task EnsureSeatAvailableAsync(Guid tenantId, CancellationToken cancellationToken);
}
