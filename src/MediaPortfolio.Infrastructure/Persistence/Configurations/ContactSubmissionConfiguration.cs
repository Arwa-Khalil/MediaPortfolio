using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MediaPortfolio.Domain.Entities;

namespace MediaPortfolio.Infrastructure.Persistence.Configurations;

public class ContactSubmissionConfiguration : IEntityTypeConfiguration<ContactSubmission>
{
    public void Configure(EntityTypeBuilder<ContactSubmission> builder)
    {
        builder.HasKey(cs => cs.Id);
        
        builder.Property(cs => cs.Name).IsRequired().HasMaxLength(255);
        builder.Property(cs => cs.Phone).IsRequired().HasMaxLength(50);
        builder.Property(cs => cs.Email).IsRequired().HasMaxLength(255);
        builder.Property(cs => cs.Message).IsRequired().HasMaxLength(2000);
    }
}
