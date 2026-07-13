namespace Chroma.Application.Modules.Activities.Dtos;

public sealed class ActivityDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? DealId { get; init; }
    public string ActivityType { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime OccurredAtUtc { get; init; }
    public int? DurationMinutes { get; init; }
}

public sealed class ActivitySearchRequest
{
    public Guid TenantId { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? DealId { get; init; }
    public string? ActivityType { get; init; }
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class ActivitySearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<ActivityDto> Items { get; init; } = [];
}

public sealed class CreateActivityRequest
{
    public Guid TenantId { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? DealId { get; init; }
    public string ActivityType { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
    public int? DurationMinutes { get; init; }
}

public sealed class UpdateActivityRequest
{
    public Guid? OwnerId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? DealId { get; init; }
    public string ActivityType { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime OccurredAtUtc { get; init; }
    public int? DurationMinutes { get; init; }
}
