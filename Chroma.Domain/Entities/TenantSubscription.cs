using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class TenantSubscription : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public string BillingInterval { get; set; } = "monthly";
    public string Status { get; set; } = "active";
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAtUtc { get; set; }

    public Tenant? Tenant { get; set; }
    public SubscriptionPlan? Plan { get; set; }
}
