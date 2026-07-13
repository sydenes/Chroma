using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Channel : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ExternalAccountId { get; set; }
    public string? SettingsJson { get; set; }
    public bool IsActive { get; set; } = true;
}
