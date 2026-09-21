using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediaPortfolio.Domain.Entities;

namespace MediaPortfolio.Infrastructure.Persistence.Configurations;

public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(a => a.Email).IsUnique();
        
        builder.Property(a => a.PasswordHash).IsRequired();
    }
}
