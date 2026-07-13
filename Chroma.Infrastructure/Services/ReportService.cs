using Chroma.Application.Abstractions;
using Chroma.Application.Modules.Reports.Dtos;
using Chroma.Application.Modules.Reports.Services;
using Microsoft.EntityFrameworkCore;

namespace Chroma.Infrastructure.Services;

public class ReportService(IApplicationDbContext dbContext, ICurrentTenant currentTenant) : IReportService
{
    public async Task<PipelineConversionReportDto> GetPipelineConversionAsync(
        PipelineConversionReportRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var pipeline = await dbContext.Pipelines
            .AsNoTracking()
            .Where(x => x.Id == request.PipelineId && x.TenantId == tenantId)
            .Select(x => new { x.Id, x.Name })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Pipeline not found.");

        var stages = await dbContext.Stages
            .AsNoTracking()
            .Where(x => x.PipelineId == request.PipelineId && x.TenantId == tenantId)
            .OrderBy(x => x.Order)
            .Select(x => new { x.Id, x.Name, x.Order, x.IsWinStage, x.IsLostStage })
            .ToListAsync(cancellationToken);

        var dealsQuery = dbContext.Deals
            .AsNoTracking()
            .Where(x => x.PipelineId == request.PipelineId && x.TenantId == tenantId);

        if (request.FromUtc.HasValue)
        {
            dealsQuery = dealsQuery.Where(x => x.CreatedAtUtc >= request.FromUtc.Value);
        }

        if (request.ToUtc.HasValue)
        {
            dealsQuery = dealsQuery.Where(x => x.CreatedAtUtc <= request.ToUtc.Value);
        }

        var deals = await dealsQuery
            .Select(x => new { x.StageId, x.Amount, x.Status })
            .ToListAsync(cancellationToken);

        var totalDeals = deals.Count;
        var wonDeals = deals.Count(d => d.Status == "won" || stages.Any(s => s.Id == d.StageId && s.IsWinStage));
        var lostDeals = deals.Count(d => d.Status == "lost" || stages.Any(s => s.Id == d.StageId && s.IsLostStage));
        var overallConversionRate = totalDeals == 0 ? 0m : Math.Round((decimal)wonDeals / totalDeals * 100m, 2);

        var stageReports = new List<PipelineConversionStageDto>();
        var previousCount = totalDeals;

        foreach (var stage in stages)
        {
            var stageDeals = deals.Where(d => d.StageId == stage.Id).ToList();
            var dealCount = stageDeals.Count;
            var totalAmount = stageDeals.Sum(d => d.Amount ?? 0m);
            var conversionRate = previousCount == 0 ? 0m : Math.Round((decimal)dealCount / previousCount * 100m, 2);

            stageReports.Add(new PipelineConversionStageDto
            {
                StageId = stage.Id,
                StageName = stage.Name,
                DealCount = dealCount,
                TotalAmount = totalAmount,
                ConversionRate = conversionRate
            });

            previousCount = dealCount > 0 ? dealCount : previousCount;
        }

        return new PipelineConversionReportDto
        {
            PipelineId = pipeline.Id,
            PipelineName = pipeline.Name,
            TotalDeals = totalDeals,
            WonDeals = wonDeals,
            LostDeals = lostDeals,
            OverallConversionRate = overallConversionRate,
            Stages = stageReports
        };
    }

    public async Task<ActivityVolumeReportDto> GetActivityVolumeAsync(
        ActivityVolumeReportRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var queryable = dbContext.Activities.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (request.OwnerId.HasValue)
        {
            queryable = queryable.Where(x => x.OwnerId == request.OwnerId.Value);
        }

        if (request.FromUtc.HasValue)
        {
            queryable = queryable.Where(x => x.OccurredAtUtc >= request.FromUtc.Value);
        }

        if (request.ToUtc.HasValue)
        {
            queryable = queryable.Where(x => x.OccurredAtUtc <= request.ToUtc.Value);
        }

        var grouped = await queryable
            .GroupBy(x => x.ActivityType)
            .Select(g => new
            {
                ActivityType = g.Key,
                Count = g.Count(),
                TotalDurationMinutes = g.Sum(x => x.DurationMinutes ?? 0)
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var items = grouped.Select(x => new ActivityVolumeItemDto
        {
            ActivityType = x.ActivityType,
            Count = x.Count,
            TotalDurationMinutes = x.TotalDurationMinutes
        }).ToList();

        return new ActivityVolumeReportDto
        {
            TotalActivities = items.Sum(x => x.Count),
            Items = items
        };
    }
}
