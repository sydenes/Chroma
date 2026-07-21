using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class TaskColumn : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string? Color { get; set; }
}
