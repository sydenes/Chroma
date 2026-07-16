using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> entity)
    {
        entity.ToTable("contacts");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.FirstName).HasMaxLength(120).IsRequired();
        entity.Property(x => x.LastName).HasMaxLength(120).IsRequired();
        entity.Property(x => x.JobTitle).HasMaxLength(120);
        entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
        entity.Property(x => x.Source).HasMaxLength(80);
        entity.Property(x => x.PotentialType).HasMaxLength(40).IsRequired();
        entity.Property(x => x.LifecycleStage).HasMaxLength(40).IsRequired();
        entity.Property(x => x.EstimatedValue).HasPrecision(18, 2);
        entity.Property(x => x.Currency).HasMaxLength(8).IsRequired();

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasIndex(x => new { x.TenantId, x.LastName, x.FirstName });
        entity.HasIndex(x => new { x.TenantId, x.PotentialType, x.LifecycleStage });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
