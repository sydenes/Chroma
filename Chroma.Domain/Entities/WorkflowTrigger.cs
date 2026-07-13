using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class WorkflowTrigger : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid WorkflowId { get; set; }
    public string TriggerType { get; set; } = string.Empty;
    public string? ConfigJson { get; set; }
}
