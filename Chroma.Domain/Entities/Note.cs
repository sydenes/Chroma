using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Note : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid? AuthorId { get; set; }
    public string OwnerType { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public string Content { get; set; } = string.Empty;
}
