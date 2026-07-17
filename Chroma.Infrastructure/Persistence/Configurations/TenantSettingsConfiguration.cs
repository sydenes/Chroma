using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class TenantSettingsConfiguration : IEntityTypeConfiguration<TenantSettings>
{
    public void Configure(EntityTypeBuilder<TenantSettings> entity)
    {
        entity.ToTable("tenant_settings");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Theme).HasMaxLength(40).IsRequired();
        entity.Property(x => x.AccentColor).HasMaxLength(40).IsRequired();
        entity.Property(x => x.Language).HasMaxLength(10).IsRequired();
        entity.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        entity.Property(x => x.TimeZone).HasMaxLength(80).IsRequired();
        entity.Property(x => x.ExtrasJson).HasColumnType("jsonb");

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasIndex(x => x.TenantId).IsUnique().HasFilter("\"IsDeleted\" = false");
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
