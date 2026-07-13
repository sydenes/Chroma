using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> entity)
    {
        entity.ToTable("permissions");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Key).HasMaxLength(120).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(500);

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasIndex(x => x.Key).IsUnique().HasFilter("\"IsDeleted\" = false");
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
