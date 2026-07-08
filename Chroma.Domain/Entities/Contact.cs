using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Contact : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? OwnerId { get; set; }
    public Guid? CompanyId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? Description { get; set; }
    public string? Source { get; set; }
    public string Status { get; set; } = "active";
}
