using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class TaskComment : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid CardId { get; set; }
    public Guid AuthorUserId { get; set; }
    public string Body { get; set; } = string.Empty;
}
