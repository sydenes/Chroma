using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Conversation : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid ChannelId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string Status { get; set; } = "open";
    public int UnreadCount { get; set; }
    public string? ExternalConversationId { get; set; }
    public DateTime? LastMessageAtUtc { get; set; }
}
