using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class CrmTask : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? OwnerId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? DealId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "pending";
    public string Priority { get; set; } = "normal";
    public DateTime? DueAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
