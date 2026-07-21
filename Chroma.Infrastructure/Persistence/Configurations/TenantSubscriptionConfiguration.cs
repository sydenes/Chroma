using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> entity)
    {
        entity.ToTable("tenant_subscriptions");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.BillingInterval).HasMaxLength(20).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
        entity.Property(x => x.StartedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.ExpiresAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasIndex(x => x.TenantId);
        entity.HasIndex(x => new { x.TenantId, x.Status });

        entity.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.Plan)
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
