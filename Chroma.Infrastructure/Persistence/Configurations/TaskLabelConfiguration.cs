using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class TaskLabelConfiguration : IEntityTypeConfiguration<TaskLabel>
{
    public void Configure(EntityTypeBuilder<TaskLabel> entity)
    {
        entity.ToTable("task_labels");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
        entity.Property(x => x.Color).HasMaxLength(40).IsRequired();
        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne<TaskBoard>()
            .WithMany()
            .HasForeignKey(x => x.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => new { x.BoardId, x.Name })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
