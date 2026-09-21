using System.Threading.Tasks;
using System.Threading;
using MediaPortfolio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediaPortfolio.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Video> Videos { get; }
    DbSet<VideoCategory> VideoCategories { get; }
    DbSet<SecondaryService> SecondaryServices { get; }
    DbSet<ContactSubmission> ContactSubmissions { get; }
    DbSet<SiteSettings> SiteSettings { get; }
    DbSet<AdminUser> AdminUsers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
