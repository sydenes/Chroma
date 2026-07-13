using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Message : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid ChannelId { get; set; }
    public string Direction { get; set; } = "IN";
    public string MessageType { get; set; } = "text";
    public string? ExternalId { get; set; }
    public string? Text { get; set; }
    public string? MediaUrl { get; set; }
    public string Status { get; set; } = "sent";
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
