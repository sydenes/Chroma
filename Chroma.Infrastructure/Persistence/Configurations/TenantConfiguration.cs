using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> entity)
    {
        entity.ToTable("tenants");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Slug).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Logo).HasMaxLength(500);
        entity.Property(x => x.Phone).HasMaxLength(50);
        entity.Property(x => x.Email).HasMaxLength(255);
        entity.Property(x => x.Website).HasMaxLength(255);
        entity.Property(x => x.TimeZone).HasMaxLength(80).IsRequired();
        entity.Property(x => x.Language).HasMaxLength(10).IsRequired();
        entity.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(40).IsRequired();

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasIndex(x => x.Slug).IsUnique().HasFilter("\"IsDeleted\" = false");
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
