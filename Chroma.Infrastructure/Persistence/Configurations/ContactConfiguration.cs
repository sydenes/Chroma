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

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasIndex(x => new { x.TenantId, x.LastName, x.FirstName });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
