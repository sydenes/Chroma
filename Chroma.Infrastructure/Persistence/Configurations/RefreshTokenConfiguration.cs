using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> entity)
    {
        entity.ToTable("refresh_tokens");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.TokenHash).HasMaxLength(500).IsRequired();
        entity.Property(x => x.ExpiresAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.RevokedAtUtc).HasColumnType("timestamptz");

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasIndex(x => x.TokenHash).IsUnique();
        entity.HasIndex(x => new { x.UserId, x.TenantId, x.ExpiresAtUtc });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
