using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public string Status { get; set; } = "scheduled";
    public string Mode { get; set; } = "office";
}
