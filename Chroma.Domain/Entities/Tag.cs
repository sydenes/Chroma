using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Tag : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
}
