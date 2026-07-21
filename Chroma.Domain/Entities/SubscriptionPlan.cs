using Chroma.Domain.Common;

namespace Chroma.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxUsers { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public string Currency { get; set; } = "TRY";
    public int SortOrder { get; set; }
    public bool IsDefault { get; set; }
    public string Status { get; set; } = "active";
}
