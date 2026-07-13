using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Channels.Dtos;
using Chroma.Application.Modules.Channels.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class ChannelService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : IChannelService
{
    public async Task<ChannelSearchResult> SearchAsync(ChannelSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.Channels.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Provider))
        {
            queryable = queryable.Where(x => x.Provider == request.Provider.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queryable = queryable.Where(x => x.Name.Contains(request.Query.Trim()));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await queryable
            .OrderBy(x => x.Name)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        return new ChannelSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<ChannelDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        return await dbContext.Channels
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ChannelDto> CreateAsync(CreateChannelRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = new Channel
        {
            TenantId = tenantId,
            Provider = request.Provider.Trim(),
            Name = request.Name.Trim(),
            ExternalAccountId = request.ExternalAccountId,
            SettingsJson = request.SettingsJson,
            IsActive = true
        };

        dbContext.Channels.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<ChannelDto?> UpdateAsync(Guid id, UpdateChannelRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Channels.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Name = request.Name.Trim();
        entity.ExternalAccountId = request.ExternalAccountId;
        entity.SettingsJson = request.SettingsJson;
        entity.IsActive = request.IsActive;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Channels.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.DeletedAtUtc = DateTime.UtcNow;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static ChannelDto ToDto(Channel entity)
    {
        return new ChannelDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Provider = entity.Provider,
            Name = entity.Name,
            ExternalAccountId = entity.ExternalAccountId,
            SettingsJson = entity.SettingsJson,
            IsActive = entity.IsActive
        };
    }

    private static Expression<Func<Channel, ChannelDto>> MapToDto()
    {
        return x => new ChannelDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            Provider = x.Provider,
            Name = x.Name,
            ExternalAccountId = x.ExternalAccountId,
            SettingsJson = x.SettingsJson,
            IsActive = x.IsActive
        };
    }
}
