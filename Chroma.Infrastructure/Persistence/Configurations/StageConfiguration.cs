using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class StageConfiguration : IEntityTypeConfiguration<Stage>
{
    public void Configure(EntityTypeBuilder<Stage> entity)
    {
        entity.ToTable("stages");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
        entity.Property(x => x.Color).HasMaxLength(20);

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne<Pipeline>()
            .WithMany()
            .HasForeignKey(x => x.PipelineId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => new { x.PipelineId, x.Name }).IsUnique().HasFilter("\"IsDeleted\" = false");
        entity.HasIndex(x => new { x.TenantId, x.PipelineId, x.Order });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
