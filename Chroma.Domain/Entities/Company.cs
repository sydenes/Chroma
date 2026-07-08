using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Company : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Sector { get; set; }
    public string? Description { get; set; }
}
