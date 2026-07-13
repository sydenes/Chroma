using Chroma.Application.Modules.Tenants.Dtos;

namespace Chroma.Application.Modules.Tenants.Services;

public interface ITenantService
{
    Task<TenantSettingsDto?> GetSettingsAsync(CancellationToken cancellationToken);
    Task<TenantSettingsDto?> UpdateSettingsAsync(UpdateTenantSettingsRequest request, CancellationToken cancellationToken);
}
