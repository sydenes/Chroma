using Chroma.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chroma.Infrastructure.Persistence.Configurations;

public class CompanyTagConfiguration : IEntityTypeConfiguration<CompanyTag>
{
    public void Configure(EntityTypeBuilder<CompanyTag> entity)
    {
        entity.ToTable("company_tags");
        entity.HasKey(x => new { x.CompanyId, x.TagId });

        entity.HasOne<Company>()
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne<Tag>()
            .WithMany()
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
