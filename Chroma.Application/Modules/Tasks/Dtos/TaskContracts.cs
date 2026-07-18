namespace Chroma.Application.Modules.Tasks.Dtos;

public sealed class CrmTaskDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? CreatedByUserId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? DealId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = "pending";
    public string Priority { get; init; } = "normal";
    public DateTime? DueAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
    public string? ContactName { get; set; }
    public string? OwnerName { get; set; }
    public string? CreatedByName { get; set; }
}

public sealed class CrmTaskSearchRequest
{
    public Guid TenantId { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? DealId { get; init; }
    public string? Status { get; init; }
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class CrmTaskSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<CrmTaskDto> Items { get; init; } = [];
}

public sealed class CreateCrmTaskRequest
{
    public Guid TenantId { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? DealId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Priority { get; init; } = "normal";
    public DateTime? DueAtUtc { get; init; }
}

public sealed class UpdateCrmTaskRequest
{
    public Guid? OwnerId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? DealId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = "pending";
    public string Priority { get; init; } = "normal";
    public DateTime? DueAtUtc { get; init; }
}
