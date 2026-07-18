namespace Chroma.Application.Modules.Conversations.Dtos;

public sealed class ConversationDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid ChannelId { get; init; }
    public Guid? ContactId { get; init; }
    public string? ContactName { get; set; }
    public Guid? AssignedUserId { get; init; }
    public Guid? PeerUserId { get; set; }
    public string? PeerUserName { get; set; }
    /// <summary>team | external</summary>
    public string Kind { get; set; } = "external";
    public string Title { get; set; } = string.Empty;
    public bool IsGroup { get; set; }
    public int ParticipantCount { get; set; }
    public IReadOnlyCollection<string> ParticipantNames { get; set; } = [];
    public string Status { get; init; } = "open";
    public int UnreadCount { get; set; }
    public string? ExternalConversationId { get; init; }
    public DateTime? LastMessageAtUtc { get; init; }
    public string? LastMessagePreview { get; init; }
}

public sealed class ConversationSearchRequest
{
    public Guid TenantId { get; init; }
    public Guid? ChannelId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? AssignedUserId { get; init; }
    /// <summary>team | external</summary>
    public string? Kind { get; init; }
    /// <summary>true ise sadece mevcut kullanıcının katıldığı konuşmalar</summary>
    public bool? MineOnly { get; init; }
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
    public Guid? ChannelId { get; init; }
    public Guid? ContactId { get; init; }
    public Guid? AssignedUserId { get; init; }
    /// <summary>Ekip sohbeti için karşı taraf kullanıcı id (1:1)</summary>
    public Guid? PeerUserId { get; init; }
    /// <summary>Grup sohbeti oluşturmak için üye kullanıcı id listesi (me hariç veya dahil)</summary>
    public IReadOnlyCollection<Guid>? MemberUserIds { get; init; }
    /// <summary>Grup adı</summary>
    public string? Title { get; init; }
    public bool IsGroup { get; init; }
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
