namespace Chroma.Application.Modules.Notes.Dtos;

public sealed class NoteDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid? AuthorId { get; init; }
    public string OwnerType { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public string Content { get; init; } = string.Empty;
}

public sealed class NoteSearchRequest
{
    public Guid TenantId { get; init; }
    public string? OwnerType { get; init; }
    public Guid? OwnerId { get; init; }
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class NoteSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<NoteDto> Items { get; init; } = [];
}

public sealed class CreateNoteRequest
{
    public Guid TenantId { get; init; }
    public Guid? AuthorId { get; init; }
    public string OwnerType { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public string Content { get; init; } = string.Empty;
}

public sealed class UpdateNoteRequest
{
    public string Content { get; init; } = string.Empty;
}
