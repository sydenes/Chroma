using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class OfferPackageConfiguration : IEntityTypeConfiguration<OfferPackage>
{
    public void Configure(EntityTypeBuilder<OfferPackage> entity)
    {
        entity.ToTable("offer_packages");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Price).HasPrecision(18, 2);
        entity.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired();

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasIndex(x => x.TenantId);
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
