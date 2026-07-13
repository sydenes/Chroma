using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class WorkflowTriggerConfiguration : IEntityTypeConfiguration<WorkflowTrigger>
{
    public void Configure(EntityTypeBuilder<WorkflowTrigger> entity)
    {
        entity.ToTable("workflow_triggers");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.TriggerType).HasMaxLength(80).IsRequired();
        entity.Property(x => x.ConfigJson).HasColumnType("jsonb");

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasOne<Workflow>()
            .WithMany()
            .HasForeignKey(x => x.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => new { x.WorkflowId, x.TriggerType });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
