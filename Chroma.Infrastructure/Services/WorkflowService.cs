using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Workflows.Dtos;
using Chroma.Application.Modules.Workflows.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class WorkflowService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : IWorkflowService
{
    public async Task<WorkflowSearchResult> SearchAsync(WorkflowSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.Workflows.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queryable = queryable.Where(x => x.Name.Contains(request.Query.Trim()));
        }

        if (request.IsActive.HasValue)
        {
            queryable = queryable.Where(x => x.IsActive == request.IsActive.Value);
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await queryable
            .OrderBy(x => x.Name)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDtoWithoutChildren())
            .ToListAsync(cancellationToken);

        return new WorkflowSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<WorkflowDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var workflow = await dbContext.Workflows
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDtoWithoutChildren())
            .FirstOrDefaultAsync(cancellationToken);

        if (workflow is null)
        {
            return null;
        }

        var triggers = await dbContext.WorkflowTriggers
            .AsNoTracking()
            .Where(x => x.WorkflowId == id && x.TenantId == tenantId)
            .Select(MapTriggerToDto())
            .ToListAsync(cancellationToken);

        var conditions = await dbContext.WorkflowConditions
            .AsNoTracking()
            .Where(x => x.WorkflowId == id && x.TenantId == tenantId)
            .OrderBy(x => x.Order)
            .Select(MapConditionToDto())
            .ToListAsync(cancellationToken);

        var actions = await dbContext.WorkflowActions
            .AsNoTracking()
            .Where(x => x.WorkflowId == id && x.TenantId == tenantId)
            .OrderBy(x => x.Order)
            .Select(MapActionToDto())
            .ToListAsync(cancellationToken);

        return new WorkflowDto
        {
            Id = workflow.Id,
            TenantId = workflow.TenantId,
            Name = workflow.Name,
            Description = workflow.Description,
            IsActive = workflow.IsActive,
            Triggers = triggers,
            Conditions = conditions,
            Actions = actions
        };
    }

    public async Task<WorkflowDto> CreateAsync(CreateWorkflowRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = new Workflow
        {
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Description = request.Description,
            IsActive = true
        };

        dbContext.Workflows.Add(entity);

        foreach (var triggerRequest in request.Triggers)
        {
            dbContext.WorkflowTriggers.Add(new WorkflowTrigger
            {
                TenantId = tenantId,
                WorkflowId = entity.Id,
                TriggerType = triggerRequest.TriggerType.Trim(),
                ConfigJson = triggerRequest.ConfigJson
            });
        }

        foreach (var conditionRequest in request.Conditions)
        {
            dbContext.WorkflowConditions.Add(new WorkflowCondition
            {
                TenantId = tenantId,
                WorkflowId = entity.Id,
                ConditionType = conditionRequest.ConditionType.Trim(),
                ConfigJson = conditionRequest.ConfigJson,
                Order = conditionRequest.Order
            });
        }

        foreach (var actionRequest in request.Actions)
        {
            dbContext.WorkflowActions.Add(new WorkflowAction
            {
                TenantId = tenantId,
                WorkflowId = entity.Id,
                ActionType = actionRequest.ActionType.Trim(),
                ConfigJson = actionRequest.ConfigJson,
                Order = actionRequest.Order
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<WorkflowDto?> UpdateAsync(Guid id, UpdateWorkflowRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Workflows.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Name = request.Name.Trim();
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Workflows.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.DeletedAtUtc = DateTime.UtcNow;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await SoftDeleteChildrenAsync(id, tenantId, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task SoftDeleteChildrenAsync(Guid workflowId, Guid tenantId, CancellationToken cancellationToken)
    {
        var triggers = await dbContext.WorkflowTriggers
            .Where(x => x.WorkflowId == workflowId && x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        foreach (var trigger in triggers)
        {
            trigger.IsDeleted = true;
            trigger.DeletedAtUtc = DateTime.UtcNow;
            trigger.UpdatedAtUtc = DateTime.UtcNow;
        }

        var conditions = await dbContext.WorkflowConditions
            .Where(x => x.WorkflowId == workflowId && x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        foreach (var condition in conditions)
        {
            condition.IsDeleted = true;
            condition.DeletedAtUtc = DateTime.UtcNow;
            condition.UpdatedAtUtc = DateTime.UtcNow;
        }

        var actions = await dbContext.WorkflowActions
            .Where(x => x.WorkflowId == workflowId && x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        foreach (var action in actions)
        {
            action.IsDeleted = true;
            action.DeletedAtUtc = DateTime.UtcNow;
            action.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    private static Expression<Func<Workflow, WorkflowDto>> MapToDtoWithoutChildren()
    {
        return x => new WorkflowDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            Name = x.Name,
            Description = x.Description,
            IsActive = x.IsActive,
            Triggers = new List<WorkflowTriggerDto>(),
            Conditions = new List<WorkflowConditionDto>(),
            Actions = new List<WorkflowActionDto>()
        };
    }

    private static Expression<Func<WorkflowTrigger, WorkflowTriggerDto>> MapTriggerToDto()
    {
        return x => new WorkflowTriggerDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            WorkflowId = x.WorkflowId,
            TriggerType = x.TriggerType,
            ConfigJson = x.ConfigJson
        };
    }

    private static Expression<Func<WorkflowCondition, WorkflowConditionDto>> MapConditionToDto()
    {
        return x => new WorkflowConditionDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            WorkflowId = x.WorkflowId,
            ConditionType = x.ConditionType,
            ConfigJson = x.ConfigJson,
            Order = x.Order
        };
    }

    private static Expression<Func<WorkflowAction, WorkflowActionDto>> MapActionToDto()
    {
        return x => new WorkflowActionDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            WorkflowId = x.WorkflowId,
            ActionType = x.ActionType,
            ConfigJson = x.ConfigJson,
            Order = x.Order
        };
    }
}
