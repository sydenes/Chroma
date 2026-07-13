using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Deals.Dtos;
using Chroma.Application.Modules.Deals.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class DealService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : IDealService
{
    public async Task<DealSearchResult> SearchAsync(DealSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.Deals.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (request.PipelineId.HasValue)
        {
            queryable = queryable.Where(x => x.PipelineId == request.PipelineId.Value);
        }

        if (request.StageId.HasValue)
        {
            queryable = queryable.Where(x => x.StageId == request.StageId.Value);
        }

        if (request.OwnerId.HasValue)
        {
            queryable = queryable.Where(x => x.OwnerId == request.OwnerId.Value);
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

        return new DealSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<DealDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        return await dbContext.Deals
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DealBoardDto?> GetBoardAsync(Guid pipelineId, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var pipeline = await dbContext.Pipelines
            .AsNoTracking()
            .Where(x => x.Id == pipelineId && x.TenantId == tenantId)
            .Select(x => new { x.Id, x.Name })
            .FirstOrDefaultAsync(cancellationToken);

        if (pipeline is null)
        {
            return null;
        }

        var stages = await dbContext.Stages
            .AsNoTracking()
            .Where(x => x.PipelineId == pipelineId && x.TenantId == tenantId)
            .OrderBy(x => x.Order)
            .Select(x => new { x.Id, x.Name, x.Order })
            .ToListAsync(cancellationToken);

        var deals = await dbContext.Deals
            .AsNoTracking()
            .Where(x => x.PipelineId == pipelineId && x.TenantId == tenantId)
            .Select(MapToDto())
            .ToListAsync(cancellationToken);

        var columns = stages.Select(stage => new DealBoardColumnDto
        {
            StageId = stage.Id,
            StageName = stage.Name,
            Order = stage.Order,
            Deals = deals.Where(d => d.StageId == stage.Id).ToList()
        }).ToList();

        return new DealBoardDto
        {
            PipelineId = pipeline.Id,
            PipelineName = pipeline.Name,
            Columns = columns
        };
    }

    public async Task<DealDto> CreateAsync(CreateDealRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        await ValidateStageAsync(tenantId, request.PipelineId, request.StageId, cancellationToken);

        var entity = new Deal
        {
            TenantId = tenantId,
            PipelineId = request.PipelineId,
            StageId = request.StageId,
            CompanyId = request.CompanyId,
            ContactId = request.ContactId,
            OwnerId = request.OwnerId,
            Title = request.Title.Trim(),
            Amount = request.Amount,
            Currency = request.Currency,
            Probability = request.Probability,
            ExpectedCloseDateUtc = request.ExpectedCloseDateUtc
        };

        dbContext.Deals.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    public async Task<DealDto?> UpdateAsync(Guid id, UpdateDealRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Deals.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.CompanyId = request.CompanyId;
        entity.ContactId = request.ContactId;
        entity.OwnerId = request.OwnerId;
        entity.Title = request.Title.Trim();
        entity.Amount = request.Amount;
        entity.Currency = request.Currency;
        entity.Probability = request.Probability;
        entity.Status = string.IsNullOrWhiteSpace(request.Status) ? entity.Status : request.Status.Trim().ToLowerInvariant();
        entity.ExpectedCloseDateUtc = request.ExpectedCloseDateUtc;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<DealDto?> MoveStageAsync(Guid id, MoveDealStageRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Deals.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        await ValidateStageAsync(tenantId, entity.PipelineId, request.StageId, cancellationToken);

        entity.StageId = request.StageId;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        var stage = await dbContext.Stages.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.StageId && x.TenantId == tenantId, cancellationToken);

        if (stage?.IsWinStage == true)
        {
            entity.Status = "won";
        }
        else if (stage?.IsLostStage == true)
        {
            entity.Status = "lost";
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Deals.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
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

    private async Task ValidateStageAsync(Guid tenantId, Guid pipelineId, Guid stageId, CancellationToken cancellationToken)
    {
        var valid = await dbContext.Stages.AsNoTracking()
            .AnyAsync(x => x.Id == stageId && x.PipelineId == pipelineId && x.TenantId == tenantId, cancellationToken);

        if (!valid)
        {
            throw new InvalidOperationException("Stage does not belong to the specified pipeline.");
        }
    }

    private static DealDto ToDto(Deal entity)
    {
        return new DealDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            PipelineId = entity.PipelineId,
            StageId = entity.StageId,
            CompanyId = entity.CompanyId,
            ContactId = entity.ContactId,
            OwnerId = entity.OwnerId,
            Title = entity.Title,
            Amount = entity.Amount,
            Currency = entity.Currency,
            Probability = entity.Probability,
            Status = entity.Status,
            ExpectedCloseDateUtc = entity.ExpectedCloseDateUtc
        };
    }

    private static Expression<Func<Deal, DealDto>> MapToDto()
    {
        return x => new DealDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            PipelineId = x.PipelineId,
            StageId = x.StageId,
            CompanyId = x.CompanyId,
            ContactId = x.ContactId,
            OwnerId = x.OwnerId,
            Title = x.Title,
            Amount = x.Amount,
            Currency = x.Currency,
            Probability = x.Probability,
            Status = x.Status,
            ExpectedCloseDateUtc = x.ExpectedCloseDateUtc
        };
    }
}
