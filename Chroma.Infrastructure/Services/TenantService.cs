using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Tenants.Dtos;
using Chroma.Application.Modules.Tenants.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chroma.Infrastructure.Services;

public class TenantService(
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant) : ITenantService
{
    public async Task<TenantSettingsDto?> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var settings = await dbContext.TenantSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

        return settings is null ? null : ToDto(settings);
    }

    public async Task<TenantSettingsDto?> UpdateSettingsAsync(UpdateTenantSettingsRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var settings = await dbContext.TenantSettings.FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        if (settings is null)
        {
            settings = new TenantSettings { TenantId = tenantId };
            dbContext.TenantSettings.Add(settings);
        }

        settings.Theme = request.Theme.Trim();
        settings.Language = request.Language.Trim();
        settings.Currency = request.Currency.Trim();
        settings.TimeZone = request.TimeZone.Trim();
        settings.ExtrasJson = request.ExtrasJson;
        settings.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(settings);
    }

    private static TenantSettingsDto ToDto(TenantSettings settings)
    {
        return new TenantSettingsDto
        {
            TenantId = settings.TenantId,
            Theme = settings.Theme,
            Language = settings.Language,
            Currency = settings.Currency,
            TimeZone = settings.TimeZone,
            ExtrasJson = settings.ExtrasJson
        };
    }
}
