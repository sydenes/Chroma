using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class ContactChannelConfiguration : IEntityTypeConfiguration<ContactChannel>
{
    public void Configure(EntityTypeBuilder<ContactChannel> entity)
    {
        entity.ToTable("contact_channels");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ChannelType).HasMaxLength(40).IsRequired();
        entity.Property(x => x.Value).HasMaxLength(255).IsRequired();

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasIndex(x => new { x.TenantId, x.ChannelType, x.Value }).IsUnique();
        entity.HasOne<Contact>().WithMany().HasForeignKey(x => x.ContactId).OnDelete(DeleteBehavior.Cascade);
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
