using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class WorkflowCondition : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid WorkflowId { get; set; }
    public string ConditionType { get; set; } = string.Empty;
    public string? ConfigJson { get; set; }
    public int Order { get; set; }
}
