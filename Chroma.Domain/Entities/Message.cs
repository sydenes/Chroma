using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Message : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid ChannelId { get; set; }
    /// <summary>
    /// Channel/provider semantics: IN = received from external party, OUT = sent via API/agent.
    /// Not used for bubble alignment in multi-party (team/group) chats — use SenderUserId.
    /// </summary>
    public string Direction { get; set; } = "IN";
    /// <summary>Tenant user who authored the message (team/group). Null for pure external inbound.</summary>
    public Guid? SenderUserId { get; set; }
    /// <summary>Denormalized display name for group UIs (avoids N+1 joins).</summary>
    public string? SenderDisplayName { get; set; }
    public string MessageType { get; set; } = "text";
    public string? ExternalId { get; set; }
    public string? Text { get; set; }
    public string? MediaUrl { get; set; }
    /// <summary>Optional attachment stored via files API.</summary>
    public Guid? FileId { get; set; }
    public string Status { get; set; } = "sent";
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
