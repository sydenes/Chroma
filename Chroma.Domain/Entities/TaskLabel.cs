using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class TaskLabel : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#64748b";
}
