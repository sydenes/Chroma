using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string Language { get; set; } = "tr";
    public string Currency { get; set; } = "TRY";
    public string Status { get; set; } = "active";
}
