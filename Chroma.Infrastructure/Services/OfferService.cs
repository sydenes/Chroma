using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Offers.Dtos;
using Chroma.Application.Modules.Offers.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class OfferService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : IOfferService
{
    public async Task<OfferPackageSearchResult> SearchAsync(OfferPackageSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.OfferPackages.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            queryable = queryable.Where(x => x.Status == request.Status.Trim().ToLowerInvariant());
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

        return new OfferPackageSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<OfferPackageDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        return await dbContext.OfferPackages
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<OfferPackageDto> CreateAsync(CreateOfferPackageRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = new OfferPackage
        {
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Description = Clean(request.Description),
            SessionCount = request.SessionCount,
            DurationMinutes = request.DurationMinutes,
            Price = request.Price,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "TRY" : request.Currency.Trim().ToUpperInvariant()
        };

        dbContext.OfferPackages.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<OfferPackageDto?> UpdateAsync(Guid id, UpdateOfferPackageRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.OfferPackages.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Name = request.Name.Trim();
        entity.Description = Clean(request.Description);
        entity.SessionCount = request.SessionCount;
        entity.DurationMinutes = request.DurationMinutes;
        entity.Price = request.Price;
        entity.Currency = string.IsNullOrWhiteSpace(request.Currency) ? entity.Currency : request.Currency.Trim().ToUpperInvariant();
        entity.Status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status.Trim().ToLowerInvariant();
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.OfferPackages.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
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

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static OfferPackageDto ToDto(OfferPackage entity)
    {
        return new OfferPackageDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            Name = entity.Name,
            Description = entity.Description,
            SessionCount = entity.SessionCount,
            DurationMinutes = entity.DurationMinutes,
            Price = entity.Price,
            Currency = entity.Currency,
            Status = entity.Status
        };
    }

    private static Expression<Func<OfferPackage, OfferPackageDto>> MapToDto()
    {
        return x => new OfferPackageDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            Name = x.Name,
            Description = x.Description,
            SessionCount = x.SessionCount,
            DurationMinutes = x.DurationMinutes,
            Price = x.Price,
            Currency = x.Currency,
            Status = x.Status
        };
    }
}
