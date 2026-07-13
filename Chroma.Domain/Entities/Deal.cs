using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Deal : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid PipelineId { get; set; }
    public Guid StageId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? ContactId { get; set; }
    public Guid? OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public int? Probability { get; set; }
    public string Status { get; set; } = "open";
    public DateTime? ExpectedCloseDateUtc { get; set; }
}
