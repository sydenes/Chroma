namespace Chroma.Application.Modules.Tags.Dtos;

public sealed class TagDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Color { get; init; }
}

public sealed class TagSearchRequest
{
    public Guid TenantId { get; init; }
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class TagSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<TagDto> Items { get; init; } = [];
}

public sealed class CreateTagRequest
{
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Color { get; init; }
}

public sealed class UpdateTagRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Color { get; init; }
}
