using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Stage : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid PipelineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? Color { get; set; }
    public bool IsWinStage { get; set; }
    public bool IsLostStage { get; set; }
}
