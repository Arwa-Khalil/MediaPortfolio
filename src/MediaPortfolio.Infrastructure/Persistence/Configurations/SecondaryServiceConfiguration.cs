using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediaPortfolio.Domain.Entities;

namespace MediaPortfolio.Infrastructure.Persistence.Configurations;

public class SecondaryServiceConfiguration : IEntityTypeConfiguration<SecondaryService>
{
    public void Configure(EntityTypeBuilder<SecondaryService> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.TitleEn).IsRequired().HasMaxLength(255);
        builder.Property(s => s.TitleAr).IsRequired().HasMaxLength(255);
        builder.Property(s => s.DescriptionEn).IsRequired().HasMaxLength(1000);
        builder.Property(s => s.DescriptionAr).IsRequired().HasMaxLength(1000);
    }
}
