using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class OutboxMessage : BaseEntity
{
    public Guid TenantId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public string Status { get; set; } = "pending";
    public DateTime? ProcessedAtUtc { get; set; }
    public string? Error { get; set; }
}
