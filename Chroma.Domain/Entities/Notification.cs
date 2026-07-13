using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string NotificationType { get; set; } = "info";
    public bool IsRead { get; set; }
    public DateTime? ReadAtUtc { get; set; }
}
