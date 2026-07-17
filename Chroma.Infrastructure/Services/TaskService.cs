using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Tasks.Dtos;
using Chroma.Application.Modules.Tasks.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class TaskService(IApplicationDbContext dbContext, ICurrentTenant currentTenant, ICurrentUser currentUser) : ITaskService
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

        await PopulateNamesAsync(items, cancellationToken);

        return new CrmTaskSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<CrmTaskDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var task = await dbContext.CrmTasks
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);

        if (task is not null)
        {
            await PopulateNamesAsync([task], cancellationToken);
        }

        return task;
    }

    public async Task<CrmTaskDto> CreateAsync(CreateCrmTaskRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var ownerId = request.OwnerId ?? currentUser.UserId;

        var entity = new CrmTask
        {
            TenantId = tenantId,
            OwnerId = ownerId,
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

        var dto = ToDto(entity);
        await PopulateNamesAsync([dto], cancellationToken);
        return dto;
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

        var dto = ToDto(entity);
        await PopulateNamesAsync([dto], cancellationToken);
        return dto;
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

    private async Task PopulateNamesAsync(
        IReadOnlyCollection<CrmTaskDto> tasks,
        CancellationToken cancellationToken)
    {
        if (tasks.Count == 0)
        {
            return;
        }

        var contactIds = tasks
            .Where(x => x.ContactId.HasValue)
            .Select(x => x.ContactId!.Value)
            .Distinct()
            .ToArray();

        if (contactIds.Length > 0)
        {
            var contacts = await dbContext.Contacts
                .AsNoTracking()
                .Where(x => contactIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.FirstName + " " + x.LastName })
                .ToListAsync(cancellationToken);

            var contactNameById = contacts.ToDictionary(x => x.Id, x => x.Name.Trim());

            foreach (var task in tasks)
            {
                if (task.ContactId is Guid contactId && contactNameById.TryGetValue(contactId, out var name))
                {
                    task.ContactName = name;
                }
            }
        }

        var ownerIds = tasks
            .Where(x => x.OwnerId.HasValue)
            .Select(x => x.OwnerId!.Value)
            .Distinct()
            .ToArray();

        if (ownerIds.Length > 0)
        {
            var owners = await dbContext.Users
                .AsNoTracking()
                .Where(x => ownerIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.FirstName + " " + x.LastName })
                .ToListAsync(cancellationToken);

            var ownerNameById = owners.ToDictionary(x => x.Id, x => x.Name.Trim());

            foreach (var task in tasks)
            {
                if (task.OwnerId is Guid ownerId && ownerNameById.TryGetValue(ownerId, out var name))
                {
                    task.OwnerName = name;
                }
            }
        }
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
