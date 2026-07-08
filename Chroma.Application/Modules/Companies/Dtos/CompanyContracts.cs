namespace Chroma.Application.Modules.Companies.Dtos;

public sealed class CompanyDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Website { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
}

public sealed class CompanySearchRequest
{
    public Guid TenantId { get; init; }
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class CompanySearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<CompanyDto> Items { get; init; } = [];
}

public sealed class CreateCompanyRequest
{
    public Guid TenantId { get; init; }
    public Guid? OwnerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Website { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
}

public sealed class UpdateCompanyRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Website { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
}
