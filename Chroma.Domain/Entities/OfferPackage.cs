using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class OfferPackage : BaseEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SessionCount { get; set; }
    public int? DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";
    public string Status { get; set; } = "active";
}
