using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class TenantSettings : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "tr";
    public string Currency { get; set; } = "TRY";
    public string TimeZone { get; set; } = "UTC";
    public string? ExtrasJson { get; set; }
}
