using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Role : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
}
