namespace Chroma.Application.Modules.Contacts.Dtos;

public sealed class ContactDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid? CompanyId { get; init; }
    public Guid? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? JobTitle { get; init; }
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Source { get; init; }
    public string PotentialType { get; init; } = "lead";
    public string LifecycleStage { get; init; } = "new";
    public decimal? EstimatedValue { get; init; }
    public string Currency { get; init; } = "TRY";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}

public sealed class ContactSearchRequest
{
    public Guid TenantId { get; init; }
    public string? Query { get; init; }
    public string? PotentialType { get; init; }
    public string? LifecycleStage { get; init; }
    public Guid? OwnerId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class ContactSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<ContactDto> Items { get; init; } = [];
}

public sealed class CreateContactRequest
{
    public Guid TenantId { get; init; }
    public Guid? OwnerId { get; init; }
    public Guid? CompanyId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? JobTitle { get; init; }
    public string? Description { get; init; }
    public string? Source { get; init; }
    public string? PotentialType { get; init; }
    public string? LifecycleStage { get; init; }
    public decimal? EstimatedValue { get; init; }
    public string? Currency { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
}

public sealed class UpdateContactRequest
{
    public Guid? CompanyId { get; init; }
    public Guid? OwnerId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? JobTitle { get; init; }
    public string? Description { get; init; }
    public string Status { get; init; } = "active";
    public string? Source { get; init; }
    public string? PotentialType { get; init; }
    public string? LifecycleStage { get; init; }
    public decimal? EstimatedValue { get; init; }
    public string? Currency { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
}
