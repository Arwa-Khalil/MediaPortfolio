using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MediaPortfolio.Domain.Entities;
using MediaPortfolio.Application.Features.Services.Commands.DeleteSecondaryService;
using MediaPortfolio.Infrastructure.Persistence;

namespace MediaPortfolio.Application.UnitTests.Features.Services.Commands;

/// <summary>
/// Verifies that DeleteSecondaryService performs a genuine HARD delete:
///   - The row is permanently removed from the SecondaryServices table.
///   - No soft-delete flag is set — the record must not exist after deletion.
/// </summary>
public class DeleteSecondaryServiceCommandHandlerTests
{
    private static MediaPortfolioDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<MediaPortfolioDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new MediaPortfolioDbContext(options);
    }

    [Fact]
    public async Task Handle_ExistingService_HardDeletesService_RowGoneFromDatabase()
    {
        // Arrange
        var dbName = $"DeleteService_{Guid.NewGuid()}";
        var serviceId = Guid.NewGuid();

        using (var seedCtx = CreateInMemoryContext(dbName))
        {
            seedCtx.SecondaryServices.Add(new SecondaryService
            {
                Id = serviceId,
                TitleEn = "Test",
                TitleAr = "اختبار",
                DescriptionEn = "Description",
                DescriptionAr = "وصف",
                DisplayOrder = 0
            });
            await seedCtx.SaveChangesAsync();
        }

        // Act
        using var ctx = CreateInMemoryContext(dbName);
        var handler = new DeleteSecondaryServiceCommandHandler(ctx);
        var result = await handler.Handle(new DeleteSecondaryServiceCommand { Id = serviceId }, CancellationToken.None);
        await ctx.SaveChangesAsync();

        // Assert — handler succeeded
        Assert.True(result.IsSuccess);

        // Assert — row is HARD deleted (must not exist in the database at all)
        using var assertCtx = CreateInMemoryContext(dbName);
        var exists = await assertCtx.SecondaryServices.AnyAsync(s => s.Id == serviceId);
        Assert.False(exists, "Secondary Service row must be completely removed (hard delete), not just flagged.");
    }

    [Fact]
    public async Task Handle_NonExistentService_ReturnsFailure_WithResourceNotFound()
    {
        var options = new DbContextOptionsBuilder<MediaPortfolioDbContext>()
            .UseInMemoryDatabase($"DeleteServiceNotFound_{Guid.NewGuid()}")
            .Options;
        using var ctx = new MediaPortfolioDbContext(options);

        var handler = new DeleteSecondaryServiceCommandHandler(ctx);
        var result = await handler.Handle(new DeleteSecondaryServiceCommand { Id = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("RESOURCE_NOT_FOUND", result.ErrorCode);
    }
}
