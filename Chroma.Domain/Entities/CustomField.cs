using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class CustomField : BaseEntity
{
    public Guid TenantId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text";
    public string? SettingsJson { get; set; }
}
