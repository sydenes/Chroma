using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Form : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool CreateContactOnSubmit { get; set; }
}
