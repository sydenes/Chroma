namespace Chroma.Application.Modules.Conversations.Dtos;

public sealed class ConversationDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid ChannelId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? AssignedUserId { get; init; }
    public string Status { get; init; } = "open";
    public int UnreadCount { get; init; }
    public string? ExternalConversationId { get; init; }
    public DateTime? LastMessageAtUtc { get; init; }
}

public sealed class ConversationSearchRequest
{
    public Guid TenantId { get; init; }
    public Guid? ChannelId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? AssignedUserId { get; init; }
    public string? Status { get; init; }
    public bool? HasUnread { get; init; }
    public string? Query { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class ConversationSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<ConversationDto> Items { get; init; } = [];
}

public sealed class CreateConversationRequest
{
    public Guid TenantId { get; init; }
    public Guid ChannelId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? AssignedUserId { get; init; }
    public string? ExternalConversationId { get; init; }
}

public sealed class UpdateConversationRequest
{
    public Guid? ContactId { get; init; }
}

public sealed class AssignConversationRequest
{
    public Guid AssignedUserId { get; init; }
}

public sealed class UpdateConversationStatusRequest
{
    public string Status { get; init; } = string.Empty;
}
