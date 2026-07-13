using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Tasks.Dtos;
using Chroma.Application.Modules.Tasks.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class TaskService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : ITaskService
{
    public async Task<CrmTaskSearchResult> SearchAsync(CrmTaskSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.CrmTasks.AsNoTracking().Where(x => x.TenantId == tenantId);

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

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            queryable = queryable.Where(x => x.Status == request.Status.Trim().ToLowerInvariant());
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queryable = queryable.Where(x => x.Title.Contains(request.Query.Trim()));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await queryable
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        return new CrmTaskSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<CrmTaskDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        return await dbContext.CrmTasks
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CrmTaskDto> CreateAsync(CreateCrmTaskRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = new CrmTask
        {
            TenantId = tenantId,
            OwnerId = request.OwnerId,
            ContactId = request.ContactId,
            CompanyId = request.CompanyId,
            DealId = request.DealId,
            Title = request.Title.Trim(),
            Description = request.Description,
            Priority = request.Priority,
            DueAtUtc = request.DueAtUtc
        };

        dbContext.CrmTasks.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<CrmTaskDto?> UpdateAsync(Guid id, UpdateCrmTaskRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.CrmTasks.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.OwnerId = request.OwnerId;
        entity.ContactId = request.ContactId;
        entity.CompanyId = request.CompanyId;
        entity.DealId = request.DealId;
        entity.Title = request.Title.Trim();
        entity.Description = request.Description;
        entity.Status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status.Trim().ToLowerInvariant();
        entity.Priority = request.Priority;
        entity.DueAtUtc = request.DueAtUtc;

        if (entity.Status == "completed" && entity.CompletedAtUtc is null)
        {
            entity.CompletedAtUtc = DateTime.UtcNow;
        }
        else if (entity.Status != "completed")
        {
            entity.CompletedAtUtc = null;
        }

        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.CrmTasks.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
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

    private static CrmTaskDto ToDto(CrmTask entity)
    {
        return new CrmTaskDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            OwnerId = entity.OwnerId,
            ContactId = entity.ContactId,
            CompanyId = entity.CompanyId,
            DealId = entity.DealId,
            Title = entity.Title,
            Description = entity.Description,
            Status = entity.Status,
            Priority = entity.Priority,
            DueAtUtc = entity.DueAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc
        };
    }

    private static Expression<Func<CrmTask, CrmTaskDto>> MapToDto()
    {
        return x => new CrmTaskDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            OwnerId = x.OwnerId,
            ContactId = x.ContactId,
            CompanyId = x.CompanyId,
            DealId = x.DealId,
            Title = x.Title,
            Description = x.Description,
            Status = x.Status,
            Priority = x.Priority,
            DueAtUtc = x.DueAtUtc,
            CompletedAtUtc = x.CompletedAtUtc
        };
    }
}
