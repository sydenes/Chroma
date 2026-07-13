using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Activity : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? OwnerId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? DealId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public int? DurationMinutes { get; set; }
}
