using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MediaPortfolio.Infrastructure.Persistence;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Domain.Entities;
using MediaPortfolio.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MediaPortfolio.API.IntegrationTests.Services;

[Collection("SharedDbCollection")]
public class ContactRetentionTests
{
    private readonly ApiWebApplicationFactory _factory;

    public ContactRetentionTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeleteOldReadSubmissionsAsync_ShouldDelete_OnlySubmissionsOlderThan30Days()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MediaPortfolioDbContext>();
        var retentionService = scope.ServiceProvider.GetRequiredService<IContactRetentionService>();

        var oldSubmission = new ContactSubmission
        {
            Id = Guid.NewGuid(),
            Name = "Test1",
            Phone = "123",
            Email = "test1@example.com",
            Message = "Hello",
            Status = ContactSubmissionStatus.Read,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-40),
            ReadAt = DateTimeOffset.UtcNow.AddDays(-31)
        };
        
        var recentSubmission = new ContactSubmission
        {
            Id = Guid.NewGuid(),
            Name = "Test2",
            Phone = "123",
            Email = "test2@example.com",
            Message = "Hello2",
            Status = ContactSubmissionStatus.Read,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
            ReadAt = DateTimeOffset.UtcNow.AddDays(-5)
        };
        
        var unreadSubmission = new ContactSubmission
        {
            Id = Guid.NewGuid(),
            Name = "Test3",
            Phone = "123",
            Email = "test3@example.com",
            Message = "Hello3",
            Status = ContactSubmissionStatus.Unread,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-40)
        };

        context.ContactSubmissions.AddRange(oldSubmission, recentSubmission, unreadSubmission);
        await context.SaveChangesAsync();

        // Act
        await retentionService.DeleteOldReadSubmissionsAsync();

        // Assert
        var remaining = await context.ContactSubmissions.ToListAsync();
        Assert.DoesNotContain(remaining, s => s.Id == oldSubmission.Id);
        Assert.Contains(remaining, s => s.Id == recentSubmission.Id);
        Assert.Contains(remaining, s => s.Id == unreadSubmission.Id);
    }
}
