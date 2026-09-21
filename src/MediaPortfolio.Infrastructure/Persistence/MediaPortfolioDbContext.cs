using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MediaPortfolio.Domain.Entities;
using MediaPortfolio.Application.Interfaces;
using System.Reflection;

namespace MediaPortfolio.Infrastructure.Persistence;

public class MediaPortfolioDbContext : DbContext, IApplicationDbContext
{
    public MediaPortfolioDbContext(DbContextOptions<MediaPortfolioDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<VideoCategory> VideoCategories => Set<VideoCategory>();
    public DbSet<SecondaryService> SecondaryServices => Set<SecondaryService>();
    public DbSet<ContactSubmission> ContactSubmissions => Set<ContactSubmission>();
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
