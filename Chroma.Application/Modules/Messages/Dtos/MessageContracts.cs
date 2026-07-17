namespace Chroma.Application.Modules.Messages.Dtos;

public sealed class MessageDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid ConversationId { get; init; }
    public Guid ChannelId { get; init; }
    public string Direction { get; init; } = "IN";
    public Guid? SenderUserId { get; init; }
    public string? SenderDisplayName { get; init; }
    public string MessageType { get; init; } = "text";
    public string? ExternalId { get; init; }
    public string? Text { get; init; }
    public string? MediaUrl { get; init; }
    public Guid? FileId { get; init; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public string Status { get; init; } = "sent";
    public DateTime SentAtUtc { get; init; }
    public DateTime? DeliveredAtUtc { get; init; }
    public DateTime? ReadAtUtc { get; init; }
}

public sealed class MessageSearchRequest
{
    public Guid TenantId { get; init; }
    public Guid ConversationId { get; init; }
    public string? Direction { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class MessageSearchResult
{
    public int TotalCount { get; init; }
    public IReadOnlyCollection<MessageDto> Items { get; init; } = [];
}

public sealed class SendOutboundMessageRequest
{
    public Guid TenantId { get; init; }
    public Guid ConversationId { get; init; }
    public Guid ChannelId { get; init; }
    public string MessageType { get; init; } = "text";
    public string? Text { get; init; }
    public string? MediaUrl { get; init; }
    public Guid? FileId { get; init; }
}
