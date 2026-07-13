using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> entity)
    {
        entity.ToTable("files");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.OwnerType).HasMaxLength(40).IsRequired();
        entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
        entity.Property(x => x.ContentType).HasMaxLength(120).IsRequired();
        entity.Property(x => x.StorageProvider).HasMaxLength(40).IsRequired();
        entity.Property(x => x.Url).HasMaxLength(1000).IsRequired();

        entity.Property(x => x.CreatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.UpdatedAtUtc).HasColumnType("timestamptz");
        entity.Property(x => x.DeletedAtUtc).HasColumnType("timestamptz");

        entity.HasIndex(x => new { x.TenantId, x.OwnerType, x.OwnerId });
        entity.HasQueryFilter(x => !x.IsDeleted);
    }
}
