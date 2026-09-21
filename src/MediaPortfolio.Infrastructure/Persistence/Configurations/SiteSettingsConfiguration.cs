using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediaPortfolio.Domain.Entities;

namespace MediaPortfolio.Infrastructure.Persistence.Configurations;

public class SiteSettingsConfiguration : IEntityTypeConfiguration<SiteSettings>
{
    public void Configure(EntityTypeBuilder<SiteSettings> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.LogoImageId).HasMaxLength(100);
        builder.Property(s => s.SnapchatUrl).HasMaxLength(255);
        builder.Property(s => s.TiktokUrl).HasMaxLength(255);
        builder.Property(s => s.InstagramUrl).HasMaxLength(255);
        builder.Property(s => s.WhatsAppNumber).HasMaxLength(50);
        
        builder.Property(s => s.WhatsAppTemplateAr).HasMaxLength(1000);
        builder.Property(s => s.WhatsAppTemplateEn).HasMaxLength(1000);
        
        // Ensure only one row exists
        builder.HasData(new SiteSettings 
        { 
            Id = 1,
            WhatsAppTemplateAr = "مرحباً، أود الاستفسار عن...",
            WhatsAppTemplateEn = "Hello, I would like to inquire about..."
        });
    }
}
