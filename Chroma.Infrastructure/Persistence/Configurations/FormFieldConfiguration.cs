using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class FormFieldConfiguration : IEntityTypeConfiguration<FormField>
{
    public void Configure(EntityTypeBuilder<FormField> entity)
    {
        entity.ToTable("form_fields");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
        entity.Property(x => x.Label).HasMaxLength(200).IsRequired();
        entity.Property(x => x.FieldType).HasMaxLength(40).IsRequired();
        entity.Property(x => x.OptionsJson).HasColumnType("jsonb");

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne<Form>()
            .WithMany()
            .HasForeignKey(x => x.FormId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => new { x.FormId, x.Name }).IsUnique().HasFilter("\"IsDeleted\" = false");
        entity.HasIndex(x => new { x.FormId, x.Order });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
