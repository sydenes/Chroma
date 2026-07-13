namespace Chroma.Application.Modules.Files.Dtos;

public sealed class FileDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string OwnerType { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string StorageProvider { get; init; } = "local";
    public string Url { get; init; } = string.Empty;
}

public sealed class FileSearchRequest
{
    public Guid TenantId { get; init; }
    public string? OwnerType { get; init; }
    public Guid? OwnerId { get; init; }
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class FileSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<FileDto> Items { get; init; } = [];
}

public sealed class CreateFileRequest
{
    public Guid TenantId { get; init; }
    public string OwnerType { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string StorageProvider { get; init; } = "local";
    public string Url { get; init; } = string.Empty;
}

public sealed class UpdateFileRequest
{
    public string FileName { get; init; } = string.Empty;
    public string OwnerType { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
}
