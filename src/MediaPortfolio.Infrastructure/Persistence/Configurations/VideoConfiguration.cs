using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediaPortfolio.Domain.Entities;

namespace MediaPortfolio.Infrastructure.Persistence.Configurations;

public class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.HasKey(v => v.Id);
        
        builder.Property(v => v.TitleAr).IsRequired().HasMaxLength(255);
        builder.Property(v => v.DescriptionAr).IsRequired().HasMaxLength(1000);
        
        builder.Property(v => v.CloudflareStreamId).IsRequired().HasMaxLength(100);
        builder.Property(v => v.CloudflareThumbnailImageId).HasMaxLength(100);
        
        builder.HasQueryFilter(v => !v.IsDeleted);
    }
}
