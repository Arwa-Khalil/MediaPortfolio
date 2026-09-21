using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Domain.Entities;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Infrastructure.Persistence.Seeding;

public static class ApplicationDbContextSeed
{
    public static async Task SeedDefaultUserAsync(MediaPortfolioDbContext context, IPasswordHasher<AdminUser> passwordHasher, string initialPassword)
    {
        if (!context.AdminUsers.Any())
        {
            var adminUser = new AdminUser
            {
                Email = "admin@sainin.com",
                MustChangePassword = true
            };
            
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, initialPassword);
            
            context.AdminUsers.Add(adminUser);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedSampleDataAsync(MediaPortfolioDbContext context)
    {
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { NameEn = "Commercial", NameAr = "تجاري" },
                new Category { NameEn = "Documentary", NameAr = "وثائقي" },
                new Category { NameEn = "Events", NameAr = "فعاليات" }
            );
            await context.SaveChangesAsync();
        }

        if (!context.SiteSettings.Any())
        {
            context.SiteSettings.Add(new SiteSettings
            {
                Id = 1,
                WhatsAppTemplateAr = "مرحباً، أود الاستفسار عن...",
                WhatsAppTemplateEn = "Hello, I would like to inquire about..."
            });
            await context.SaveChangesAsync();
        }
    }
}
