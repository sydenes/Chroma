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
    public string Category { get; init; } = "document";
    public string StorageProvider { get; init; } = "local";
    public string StorageKey { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public Guid? UploadedByUserId { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public sealed class FileSearchRequest
{
    public Guid TenantId { get; init; }
    public string? OwnerType { get; init; }
    public Guid? OwnerId { get; init; }
    public string? Category { get; init; }
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
    public string Category { get; init; } = "document";
    public string StorageProvider { get; init; } = "local";
    public string Url { get; init; } = string.Empty;
}

public sealed class UpdateFileRequest
{
    public string FileName { get; init; } = string.Empty;
    public string OwnerType { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public string? Category { get; init; }
}

public sealed class UploadFileRequest
{
    public string OwnerType { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public string? Category { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public required Stream Content { get; init; }
}

public sealed class FileDownloadResult
{
    public required Stream Stream { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
}
