using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Pipeline : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}
