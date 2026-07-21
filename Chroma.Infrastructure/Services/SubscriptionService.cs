using Chroma.Application.Abstractions;
using Chroma.Application.Common.Exceptions;
using Chroma.Application.Modules.Subscriptions.Dtos;
using Chroma.Application.Modules.Subscriptions.Services;
using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chroma.Infrastructure.Services;

public class SubscriptionService(
    IApplicationDbContext dbContext,
    ICurrentTenant currentTenant) : ISubscriptionService
{
    public async Task<IReadOnlyCollection<SubscriptionPlanDto>> ListPlansAsync(CancellationToken cancellationToken)
    {
        var plans = await dbContext.SubscriptionPlans
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return plans.Select(MapPlan).ToArray();
    }

    public async Task<SubscriptionPlanDto> CreatePlanAsync(
        CreateSubscriptionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var code = NormalizeCode(request.Code);
        var exists = await dbContext.SubscriptionPlans.AnyAsync(x => x.Code == code, cancellationToken);
        if (exists)
        {
            throw new AppException("subscriptions.codeExists", "A plan with this code already exists.");
        }

        if (request.IsDefault)
        {
            await ClearDefaultFlagsAsync(cancellationToken);
        }

        var plan = new SubscriptionPlan
        {
            Code = code,
            Name = RequireName(request.Name),
            Description = Clean(request.Description),
            MaxUsers = RequireMaxUsers(request.MaxUsers),
            MonthlyPrice = RequirePrice(request.MonthlyPrice),
            YearlyPrice = RequirePrice(request.YearlyPrice),
            Currency = NormalizeCurrency(request.Currency),
            SortOrder = request.SortOrder,
            IsDefault = request.IsDefault,
            Status = "active"
        };

        dbContext.SubscriptionPlans.Add(plan);
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapPlan(plan);
    }

    public async Task<SubscriptionPlanDto?> UpdatePlanAsync(
        Guid id,
        UpdateSubscriptionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var plan = await dbContext.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (plan is null) return null;

        if (request.IsDefault && !plan.IsDefault)
        {
            await ClearDefaultFlagsAsync(cancellationToken);
        }

        plan.Name = RequireName(request.Name);
        plan.Description = Clean(request.Description);
        plan.MaxUsers = RequireMaxUsers(request.MaxUsers);
        plan.MonthlyPrice = RequirePrice(request.MonthlyPrice);
        plan.YearlyPrice = RequirePrice(request.YearlyPrice);
        plan.Currency = NormalizeCurrency(request.Currency);
        plan.SortOrder = request.SortOrder;
        plan.IsDefault = request.IsDefault;
        plan.Status = string.IsNullOrWhiteSpace(request.Status)
            ? plan.Status
            : request.Status.Trim().ToLowerInvariant();
        plan.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapPlan(plan);
    }

    public async Task<bool> DeletePlanAsync(Guid id, CancellationToken cancellationToken)
    {
        var plan = await dbContext.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (plan is null) return false;

        var inUse = await dbContext.TenantSubscriptions
            .AnyAsync(x => x.PlanId == id && x.Status == "active", cancellationToken);
        if (inUse)
        {
            throw new AppException(
                "subscriptions.planInUse",
                "Cannot delete a plan that is assigned to an active subscription.");
        }

        plan.IsDeleted = true;
        plan.DeletedAtUtc = DateTime.UtcNow;
        plan.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<TenantSubscriptionDto> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var subscription = await EnsureTenantSubscriptionAsync(tenantId, cancellationToken);
        return await MapSubscriptionAsync(subscription, cancellationToken);
    }

    public async Task<TenantSubscriptionDto> AssignCurrentAsync(
        AssignTenantSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = RequireTenant();
        var interval = NormalizeInterval(request.BillingInterval);

        var plan = await dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(x => x.Id == request.PlanId && x.Status == "active", cancellationToken)
            ?? throw new AppException("subscriptions.planNotFound", "Subscription plan not found.", 404);

        var activeUsers = await CountActiveUsersAsync(tenantId, cancellationToken);
        if (activeUsers > plan.MaxUsers)
        {
            throw new AppException(
                "subscriptions.planTooSmall",
                $"This plan supports up to {plan.MaxUsers} users, but the workspace already has {activeUsers} active users.");
        }

        var existing = await dbContext.TenantSubscriptions
            .Where(x => x.TenantId == tenantId && x.Status == "active")
            .ToListAsync(cancellationToken);

        foreach (var item in existing)
        {
            item.Status = "cancelled";
            item.UpdatedAtUtc = DateTime.UtcNow;
            item.ExpiresAtUtc ??= DateTime.UtcNow;
        }

        var subscription = new TenantSubscription
        {
            TenantId = tenantId,
            PlanId = plan.Id,
            BillingInterval = interval,
            Status = "active",
            StartedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = interval == "yearly" ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddMonths(1),
            Plan = plan
        };

        dbContext.TenantSubscriptions.Add(subscription);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await MapSubscriptionAsync(subscription, cancellationToken);
    }

    public async Task EnsureSeatAvailableAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var subscription = await EnsureTenantSubscriptionAsync(tenantId, cancellationToken);
        var plan = subscription.Plan
            ?? await dbContext.SubscriptionPlans.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == subscription.PlanId, cancellationToken)
            ?? throw new AppException("subscriptions.planNotFound", "Subscription plan not found.", 404);

        var activeUsers = await CountActiveUsersAsync(tenantId, cancellationToken);
        if (activeUsers >= plan.MaxUsers)
        {
            throw new AppException(
                "users.seatLimitReached",
                $"User limit reached for the current plan ({plan.MaxUsers}).");
        }
    }

    private async Task<TenantSubscription> EnsureTenantSubscriptionAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var subscription = await dbContext.TenantSubscriptions
            .Include(x => x.Plan)
            .Where(x => x.TenantId == tenantId && x.Status == "active")
            .OrderByDescending(x => x.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription is not null)
        {
            return subscription;
        }

        var defaultPlan = await dbContext.SubscriptionPlans
            .Where(x => x.Status == "active")
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.SortOrder)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new AppException("subscriptions.planNotFound", "No subscription plan is available.", 404);

        subscription = new TenantSubscription
        {
            TenantId = tenantId,
            PlanId = defaultPlan.Id,
            BillingInterval = "monthly",
            Status = "active",
            StartedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMonths(1),
            Plan = defaultPlan
        };

        dbContext.TenantSubscriptions.Add(subscription);
        await dbContext.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    private async Task ClearDefaultFlagsAsync(CancellationToken cancellationToken)
    {
        var defaults = await dbContext.SubscriptionPlans
            .Where(x => x.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var plan in defaults)
        {
            plan.IsDefault = false;
            plan.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    private Task<int> CountActiveUsersAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return dbContext.UserTenants
            .CountAsync(x => x.TenantId == tenantId && x.Status == "active", cancellationToken);
    }

    private async Task<TenantSubscriptionDto> MapSubscriptionAsync(
        TenantSubscription subscription,
        CancellationToken cancellationToken)
    {
        var plan = subscription.Plan
            ?? await dbContext.SubscriptionPlans.AsNoTracking()
                .FirstAsync(x => x.Id == subscription.PlanId, cancellationToken);

        var activeUsers = await CountActiveUsersAsync(subscription.TenantId, cancellationToken);
        var currentPrice = subscription.BillingInterval == "yearly" ? plan.YearlyPrice : plan.MonthlyPrice;

        return new TenantSubscriptionDto
        {
            Id = subscription.Id,
            TenantId = subscription.TenantId,
            PlanId = plan.Id,
            PlanCode = plan.Code,
            PlanName = plan.Name,
            PlanDescription = plan.Description,
            MaxUsers = plan.MaxUsers,
            ActiveUsers = activeUsers,
            RemainingSeats = Math.Max(0, plan.MaxUsers - activeUsers),
            MonthlyPrice = plan.MonthlyPrice,
            YearlyPrice = plan.YearlyPrice,
            Currency = plan.Currency,
            BillingInterval = subscription.BillingInterval,
            CurrentPrice = currentPrice,
            Status = subscription.Status,
            StartedAtUtc = subscription.StartedAtUtc,
            ExpiresAtUtc = subscription.ExpiresAtUtc
        };
    }

    private Guid RequireTenant()
    {
        return currentTenant.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");
    }

    private static SubscriptionPlanDto MapPlan(SubscriptionPlan plan) => new()
    {
        Id = plan.Id,
        Code = plan.Code,
        Name = plan.Name,
        Description = plan.Description,
        MaxUsers = plan.MaxUsers,
        MonthlyPrice = plan.MonthlyPrice,
        YearlyPrice = plan.YearlyPrice,
        Currency = plan.Currency,
        SortOrder = plan.SortOrder,
        IsDefault = plan.IsDefault,
        Status = plan.Status
    };

    private static string RequireName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new AppException("subscriptions.nameRequired", "Plan name is required.");
        }

        return value.Trim();
    }

    private static string NormalizeCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new AppException("subscriptions.codeRequired", "Plan code is required.");
        }

        return value.Trim().ToLowerInvariant().Replace(' ', '-');
    }

    private static int RequireMaxUsers(int value)
    {
        if (value < 1)
        {
            throw new AppException("subscriptions.maxUsersPositive", "Max users must be at least 1.");
        }

        return value;
    }

    private static decimal RequirePrice(decimal value)
    {
        if (value < 0)
        {
            throw new AppException("subscriptions.priceNonNegative", "Price cannot be negative.");
        }

        return value;
    }

    private static string NormalizeCurrency(string? value)
        => string.IsNullOrWhiteSpace(value) ? "TRY" : value.Trim().ToUpperInvariant();

    private static string NormalizeInterval(string? value)
    {
        var interval = string.IsNullOrWhiteSpace(value) ? "monthly" : value.Trim().ToLowerInvariant();
        if (interval is not ("monthly" or "yearly"))
        {
            throw new AppException("subscriptions.intervalInvalid", "Billing interval must be monthly or yearly.");
        }

        return interval;
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
