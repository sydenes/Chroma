using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class TaskBoard : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Title { get; set; } = "Görevler";
    public bool IsDefault { get; set; } = true;
}
