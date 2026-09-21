using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MediaPortfolio.Infrastructure.Persistence;

namespace MediaPortfolio.Infrastructure.Services;

public class ContactRetentionService : IContactRetentionService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ContactRetentionService> _logger;

    public ContactRetentionService(IServiceProvider serviceProvider, ILogger<ContactRetentionService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DeleteOldReadSubmissionsAsync(CancellationToken cancellationToken = default)
    {
        // Resolve a new scope since Hangfire will invoke this in a background thread
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediaPortfolioDbContext>();
        
        var cutoff = DateTimeOffset.UtcNow.AddDays(-30);

        var deletedCount = await context.ContactSubmissions
            .Where(cs => cs.Status == ContactSubmissionStatus.Read && cs.ReadAt <= cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedCount > 0)
        {
            _logger.LogInformation("Deleted {Count} old read contact submissions.", deletedCount);
        }
    }
}
