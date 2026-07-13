using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Activities.Dtos;
using Chroma.Application.Modules.Activities.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class ActivityService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : IActivityService
{
    public async Task<ActivitySearchResult> SearchAsync(ActivitySearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.Activities.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (request.OwnerId.HasValue)
        {
            queryable = queryable.Where(x => x.OwnerId == request.OwnerId.Value);
        }

        if (request.ContactId.HasValue)
        {
            queryable = queryable.Where(x => x.ContactId == request.ContactId.Value);
        }

        if (request.CompanyId.HasValue)
        {
            queryable = queryable.Where(x => x.CompanyId == request.CompanyId.Value);
        }

        if (request.DealId.HasValue)
        {
            queryable = queryable.Where(x => x.DealId == request.DealId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.ActivityType))
        {
            queryable = queryable.Where(x => x.ActivityType == request.ActivityType.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queryable = queryable.Where(x => x.Subject.Contains(request.Query.Trim()));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await queryable
            .OrderByDescending(x => x.OccurredAtUtc)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        return new ActivitySearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<ActivityDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        return await dbContext.Activities
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ActivityDto> CreateAsync(CreateActivityRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = new Activity
        {
            TenantId = tenantId,
            OwnerId = request.OwnerId,
            ContactId = request.ContactId,
            CompanyId = request.CompanyId,
            DealId = request.DealId,
            ActivityType = request.ActivityType.Trim(),
            Subject = request.Subject.Trim(),
            Description = request.Description,
            OccurredAtUtc = request.OccurredAtUtc,
            DurationMinutes = request.DurationMinutes
        };

        dbContext.Activities.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<ActivityDto?> UpdateAsync(Guid id, UpdateActivityRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Activities.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.OwnerId = request.OwnerId;
        entity.ContactId = request.ContactId;
        entity.CompanyId = request.CompanyId;
        entity.DealId = request.DealId;
        entity.ActivityType = request.ActivityType.Trim();
        entity.Subject = request.Subject.Trim();
        entity.Description = request.Description;
        entity.OccurredAtUtc = request.OccurredAtUtc;
        entity.DurationMinutes = request.DurationMinutes;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Activities.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
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

    private static ActivityDto ToDto(Activity entity)
    {
        return new ActivityDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            OwnerId = entity.OwnerId,
            ContactId = entity.ContactId,
            CompanyId = entity.CompanyId,
            DealId = entity.DealId,
            ActivityType = entity.ActivityType,
            Subject = entity.Subject,
            Description = entity.Description,
            OccurredAtUtc = entity.OccurredAtUtc,
            DurationMinutes = entity.DurationMinutes
        };
    }

    private static Expression<Func<Activity, ActivityDto>> MapToDto()
    {
        return x => new ActivityDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            OwnerId = x.OwnerId,
            ContactId = x.ContactId,
            CompanyId = x.CompanyId,
            DealId = x.DealId,
            ActivityType = x.ActivityType,
            Subject = x.Subject,
            Description = x.Description,
            OccurredAtUtc = x.OccurredAtUtc,
            DurationMinutes = x.DurationMinutes
        };
    }
}
