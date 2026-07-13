namespace Chroma.Application.Modules.Reports.Dtos;

public sealed class PipelineConversionReportRequest
{
    public Guid TenantId { get; init; }
    public Guid PipelineId { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
}

public sealed class PipelineConversionStageDto
{
    public Guid StageId { get; init; }
    public string StageName { get; init; } = string.Empty;
    public int DealCount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal ConversionRate { get; init; }
}

public sealed class PipelineConversionReportDto
{
    public Guid PipelineId { get; init; }
    public string PipelineName { get; init; } = string.Empty;
    public int TotalDeals { get; init; }
    public int WonDeals { get; init; }
    public int LostDeals { get; init; }
    public decimal OverallConversionRate { get; init; }
    public IReadOnlyCollection<PipelineConversionStageDto> Stages { get; init; } = [];
}

public sealed class ActivityVolumeReportRequest
{
    public Guid TenantId { get; init; }
    public Guid? OwnerId { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
}

public sealed class ActivityVolumeItemDto
{
    public string ActivityType { get; init; } = string.Empty;
    public int Count { get; init; }
    public int TotalDurationMinutes { get; init; }
}

public sealed class ActivityVolumeReportDto
{
    public int TotalActivities { get; init; }
    public IReadOnlyCollection<ActivityVolumeItemDto> Items { get; init; } = [];
}
