using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class TaskCard : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid BoardId { get; set; }
    public Guid ColumnId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? AssigneeUserId { get; set; }
    /// <summary>Optional related potential/contact.</summary>
    public Guid? ContactId { get; set; }
    public string Priority { get; set; } = "normal";
    public DateTime? DueAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
