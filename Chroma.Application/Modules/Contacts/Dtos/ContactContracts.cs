namespace Chroma.Application.Modules.Contacts.Dtos;

public sealed class ContactDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid? CompanyId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Source { get; init; }
}

public sealed class ContactSearchRequest
{
    public Guid TenantId { get; init; }
    public string? Query { get; init; }
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
    public string? Source { get; init; }
}

public sealed class UpdateContactRequest
{
    public Guid? CompanyId { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Status { get; init; } = "active";
    public string? Source { get; init; }
}
