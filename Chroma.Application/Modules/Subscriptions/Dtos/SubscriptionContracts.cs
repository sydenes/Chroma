namespace Chroma.Application.Modules.Subscriptions.Dtos;

public sealed class SubscriptionPlanDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int MaxUsers { get; init; }
    public decimal MonthlyPrice { get; init; }
    public decimal YearlyPrice { get; init; }
    public string Currency { get; init; } = "TRY";
    public int SortOrder { get; init; }
    public bool IsDefault { get; init; }
    public string Status { get; init; } = "active";
}

public sealed class TenantSubscriptionDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid PlanId { get; init; }
    public string PlanCode { get; init; } = string.Empty;
    public string PlanName { get; init; } = string.Empty;
    public string? PlanDescription { get; init; }
    public int MaxUsers { get; init; }
    public int ActiveUsers { get; init; }
    public int RemainingSeats { get; init; }
    public decimal MonthlyPrice { get; init; }
    public decimal YearlyPrice { get; init; }
    public string Currency { get; init; } = "TRY";
    public string BillingInterval { get; init; } = "monthly";
    public decimal CurrentPrice { get; init; }
    public string Status { get; init; } = "active";
    public DateTime StartedAtUtc { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
}

public sealed class CreateSubscriptionPlanRequest
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int MaxUsers { get; init; }
    public decimal MonthlyPrice { get; init; }
    public decimal YearlyPrice { get; init; }
    public string Currency { get; init; } = "TRY";
    public int SortOrder { get; init; }
    public bool IsDefault { get; init; }
}

public sealed class UpdateSubscriptionPlanRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int MaxUsers { get; init; }
    public decimal MonthlyPrice { get; init; }
    public decimal YearlyPrice { get; init; }
    public string Currency { get; init; } = "TRY";
    public int SortOrder { get; init; }
    public bool IsDefault { get; init; }
    public string Status { get; init; } = "active";
}

public sealed class AssignTenantSubscriptionRequest
{
    public Guid PlanId { get; init; }
    public string BillingInterval { get; init; } = "monthly";
}
