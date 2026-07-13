using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> entity)
    {
        entity.ToTable("channels");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Provider).HasMaxLength(40).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
        entity.Property(x => x.ExternalAccountId).HasMaxLength(255);
        entity.Property(x => x.SettingsJson).HasColumnType("jsonb");

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasIndex(x => new { x.TenantId, x.Provider, x.ExternalAccountId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"ExternalAccountId\" IS NOT NULL");
        entity.HasIndex(x => new { x.TenantId, x.IsActive });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
