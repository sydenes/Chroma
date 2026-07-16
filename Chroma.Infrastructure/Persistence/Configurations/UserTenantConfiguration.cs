using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class UserTenantConfiguration : IEntityTypeConfiguration<UserTenant>
{
    public void Configure(EntityTypeBuilder<UserTenant> entity)
    {
        entity.ToTable("user_tenants");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Status).HasMaxLength(40).IsRequired();

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => new { x.UserId, x.TenantId }).IsUnique().HasFilter("\"IsDeleted\" = false");
        entity.HasIndex(x => new { x.TenantId, x.Status });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
