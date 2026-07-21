using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> entity)
    {
        entity.ToTable("subscription_plans");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Code).HasMaxLength(40).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(500);
        entity.Property(x => x.MonthlyPrice).HasPrecision(18, 2);
        entity.Property(x => x.YearlyPrice).HasPrecision(18, 2);
        entity.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired();

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasIndex(x => x.Code).IsUnique().HasFilter("\"IsDeleted\" = false");
        entity.HasIndex(x => x.SortOrder);
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
