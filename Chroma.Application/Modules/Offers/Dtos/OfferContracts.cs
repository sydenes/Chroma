namespace Chroma.Application.Modules.Offers.Dtos;

public sealed class OfferPackageDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int SessionCount { get; init; }
    public int? DurationMinutes { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = "TRY";
    public string Status { get; init; } = "active";
}

public sealed class OfferPackageSearchRequest
{
    public Guid TenantId { get; init; }
    public string? Status { get; init; }
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class OfferPackageSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<OfferPackageDto> Items { get; init; } = [];
}

public sealed class CreateOfferPackageRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int SessionCount { get; init; }
    public int? DurationMinutes { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = "TRY";
}

public sealed class UpdateOfferPackageRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int SessionCount { get; init; }
    public int? DurationMinutes { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = "TRY";
    public string Status { get; init; } = "active";
}
