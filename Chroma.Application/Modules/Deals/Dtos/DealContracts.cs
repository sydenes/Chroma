namespace Chroma.Application.Modules.Deals.Dtos;

public sealed class DealDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid PipelineId { get; init; }
    public Guid StageId { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? OwnerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal? Amount { get; init; }
    public string Currency { get; init; } = "TRY";
    public int? Probability { get; init; }
    public string Status { get; init; } = "open";
    public DateTime? ExpectedCloseDateUtc { get; init; }
}

public sealed class DealSearchRequest
{
    public Guid TenantId { get; init; }
    public Guid? PipelineId { get; init; }
    public Guid? StageId { get; init; }
    public Guid? OwnerId { get; init; }
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class DealSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<DealDto> Items { get; init; } = [];
}

public sealed class DealBoardColumnDto
{
    public Guid StageId { get; init; }
    public string StageName { get; init; } = string.Empty;
    public int Order { get; init; }
    public IReadOnlyCollection<DealDto> Deals { get; init; } = [];
}

public sealed class DealBoardDto
{
    public Guid PipelineId { get; init; }
    public string PipelineName { get; init; } = string.Empty;
    public IReadOnlyCollection<DealBoardColumnDto> Columns { get; init; } = [];
}

public sealed class CreateDealRequest
{
    public Guid TenantId { get; init; }
    public Guid PipelineId { get; init; }
    public Guid StageId { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? OwnerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal? Amount { get; init; }
    public string Currency { get; init; } = "TRY";
    public int? Probability { get; init; }
    public DateTime? ExpectedCloseDateUtc { get; init; }
}

public sealed class UpdateDealRequest
{
    public Guid? CompanyId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? OwnerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal? Amount { get; init; }
    public string Currency { get; init; } = "TRY";
    public int? Probability { get; init; }
    public string Status { get; init; } = "open";
    public DateTime? ExpectedCloseDateUtc { get; init; }
}

public sealed class MoveDealStageRequest
{
    public Guid StageId { get; init; }
}
