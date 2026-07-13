using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Pipelines.Dtos;
using Chroma.Application.Modules.Pipelines.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Chroma.Infrastructure.Services;

public class PipelineService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : IPipelineService
{
    public async Task<PipelineSearchResult> SearchAsync(PipelineSearchRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.Pipelines.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            queryable = queryable.Where(x => x.Name.Contains(request.Query.Trim()));
        }

        var totalCount = await queryable.CountAsync(cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;

        var items = await queryable
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Name)
            .Skip(skip)
            .Take(request.PageSize)
            .Select(MapToDtoWithoutStages())
            .ToListAsync(cancellationToken);

        return new PipelineSearchResult { TotalCount = totalCount, Items = items };
    }

    public async Task<PipelineDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var pipeline = await dbContext.Pipelines
            .AsNoTracking()
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(MapToDtoWithoutStages())
            .FirstOrDefaultAsync(cancellationToken);

        if (pipeline is null)
        {
            return null;
        }

        var stages = await dbContext.Stages
            .AsNoTracking()
            .Where(x => x.PipelineId == id && x.TenantId == tenantId)
            .OrderBy(x => x.Order)
            .Select(MapStageToDto())
            .ToListAsync(cancellationToken);

        return new PipelineDto
        {
            Id = pipeline.Id,
            TenantId = pipeline.TenantId,
            Name = pipeline.Name,
            Order = pipeline.Order,
            Stages = stages
        };
    }

    public async Task<PipelineDto> CreateAsync(CreatePipelineRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = new Pipeline
        {
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Order = request.Order
        };

        dbContext.Pipelines.Add(entity);

        foreach (var stageRequest in request.Stages)
        {
            dbContext.Stages.Add(new Stage
            {
                TenantId = tenantId,
                PipelineId = entity.Id,
                Name = stageRequest.Name.Trim(),
                Order = stageRequest.Order,
                Color = stageRequest.Color,
                IsWinStage = stageRequest.IsWinStage,
                IsLostStage = stageRequest.IsLostStage
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return (await GetByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<PipelineDto?> UpdateAsync(Guid id, UpdatePipelineRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Pipelines.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Name = request.Name.Trim();
        entity.Order = request.Order;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Pipelines.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.DeletedAtUtc = DateTime.UtcNow;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        var stages = await dbContext.Stages
            .Where(x => x.PipelineId == id && x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        foreach (var stage in stages)
        {
            stage.IsDeleted = true;
            stage.DeletedAtUtc = DateTime.UtcNow;
            stage.UpdatedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<StageDto> CreateStageAsync(Guid pipelineId, CreateStageRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var pipelineExists = await dbContext.Pipelines.AsNoTracking()
            .AnyAsync(x => x.Id == pipelineId && x.TenantId == tenantId, cancellationToken);

        if (!pipelineExists)
        {
            throw new InvalidOperationException("Pipeline not found.");
        }

        var entity = new Stage
        {
            TenantId = tenantId,
            PipelineId = pipelineId,
            Name = request.Name.Trim(),
            Order = request.Order,
            Color = request.Color,
            IsWinStage = request.IsWinStage,
            IsLostStage = request.IsLostStage
        };

        dbContext.Stages.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToStageDto(entity);
    }

    public async Task<StageDto?> UpdateStageAsync(Guid pipelineId, Guid stageId, UpdateStageRequest request, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Stages
            .FirstOrDefaultAsync(x => x.Id == stageId && x.PipelineId == pipelineId && x.TenantId == tenantId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Name = request.Name.Trim();
        entity.Order = request.Order;
        entity.Color = request.Color;
        entity.IsWinStage = request.IsWinStage;
        entity.IsLostStage = request.IsLostStage;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToStageDto(entity);
    }

    public async Task<bool> DeleteStageAsync(Guid pipelineId, Guid stageId, CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var entity = await dbContext.Stages
            .FirstOrDefaultAsync(x => x.Id == stageId && x.PipelineId == pipelineId && x.TenantId == tenantId, cancellationToken);

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

    private static StageDto ToStageDto(Stage entity)
    {
        return new StageDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            PipelineId = entity.PipelineId,
            Name = entity.Name,
            Order = entity.Order,
            Color = entity.Color,
            IsWinStage = entity.IsWinStage,
            IsLostStage = entity.IsLostStage
        };
    }

    private static Expression<Func<Stage, StageDto>> MapStageToDto()
    {
        return x => new StageDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            PipelineId = x.PipelineId,
            Name = x.Name,
            Order = x.Order,
            Color = x.Color,
            IsWinStage = x.IsWinStage,
            IsLostStage = x.IsLostStage
        };
    }

    private static Expression<Func<Pipeline, PipelineDto>> MapToDtoWithoutStages()
    {
        return x => new PipelineDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            Name = x.Name,
            Order = x.Order,
            Stages = new List<StageDto>()
        };
    }
}
