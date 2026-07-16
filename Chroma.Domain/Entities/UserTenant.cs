using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class UserTenant : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public string Status { get; set; } = "active";
    public bool IsDefault { get; set; }
}
