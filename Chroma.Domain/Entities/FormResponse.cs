using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class FormResponse : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid FormId { get; set; }
    public Guid? ContactId { get; set; }
    public string JsonData { get; set; } = "{}";
}
