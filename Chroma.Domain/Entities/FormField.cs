using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class FormField : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid FormId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text";
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public string? OptionsJson { get; set; }
}
