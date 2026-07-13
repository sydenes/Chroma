using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class CustomFieldValueConfiguration : IEntityTypeConfiguration<CustomFieldValue>
{
    public void Configure(EntityTypeBuilder<CustomFieldValue> entity)
    {
        entity.ToTable("custom_field_values");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Value).IsRequired();

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne<CustomField>()
            .WithMany()
            .HasForeignKey(x => x.FieldId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => new { x.FieldId, x.EntityId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        entity.HasIndex(x => new { x.TenantId, x.EntityId });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
