namespace Chroma.Domain.Entities;

/// <summary>Join table between cards and labels (no soft-delete).</summary>
public class TaskCardLabel
{
    public Guid CardId { get; set; }
    public Guid LabelId { get; set; }
}
