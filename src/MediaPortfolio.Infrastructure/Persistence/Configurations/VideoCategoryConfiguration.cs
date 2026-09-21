using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediaPortfolio.Domain.Entities;

namespace MediaPortfolio.Infrastructure.Persistence.Configurations;

public class VideoCategoryConfiguration : IEntityTypeConfiguration<VideoCategory>
{
    public void Configure(EntityTypeBuilder<VideoCategory> builder)
    {
        builder.HasKey(vc => new { vc.VideoId, vc.CategoryId });

        builder.HasOne(vc => vc.Video)
            .WithMany(v => v.VideoCategories)
            .HasForeignKey(vc => vc.VideoId);

        builder.HasOne(vc => vc.Category)
            .WithMany(c => c.VideoCategories)
            .HasForeignKey(vc => vc.CategoryId);

        // Match Category's soft-delete global query filter so EF doesn't warn about
        // "required end of a relationship with a global query filter defined".
        builder.HasQueryFilter(vc => !vc.Category.IsDeleted);
    }
}
