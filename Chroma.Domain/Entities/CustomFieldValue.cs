using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class CustomFieldValue : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid FieldId { get; set; }
    public Guid EntityId { get; set; }
    public string Value { get; set; } = string.Empty;
}
