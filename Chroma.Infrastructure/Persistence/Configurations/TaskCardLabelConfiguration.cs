using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class TaskCardLabelConfiguration : IEntityTypeConfiguration<TaskCardLabel>
{
    public void Configure(EntityTypeBuilder<TaskCardLabel> entity)
    {
        entity.ToTable("task_card_labels");
        entity.HasKey(x => new { x.CardId, x.LabelId });

        entity.HasOne<TaskCard>()
            .WithMany()
            .HasForeignKey(x => x.CardId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne<TaskLabel>()
            .WithMany()
            .HasForeignKey(x => x.LabelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
