using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MediaPortfolio.Domain.Entities;
using MediaPortfolio.Domain.Enums;
using MediaPortfolio.Application.Features.Catalog.Commands.DeleteCategory;
using MediaPortfolio.Infrastructure.Persistence;

namespace MediaPortfolio.Application.UnitTests.Features.Catalog.Commands;

/// <summary>
/// Verifies the non-cascade Category deletion rule:
///   - Category is soft-deleted (IsDeleted = true)
///   - VideoCategory join rows for that category are removed
///   - Videos themselves are NOT deleted
/// </summary>
public class DeleteCategoryCommandHandlerTests
{
    private static MediaPortfolioDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<MediaPortfolioDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new MediaPortfolioDbContext(options);
    }

    [Fact]
    public async Task Handle_ExistingCategory_SoftDeletesCategoryAndRemovesJoins_ButDoesNotCascadeToVideos()
    {
        // Arrange — isolated InMemory database per test
        var dbName = $"DeleteCategory_{Guid.NewGuid()}";

        var categoryId = Guid.NewGuid();
        var videoId = Guid.NewGuid();

        using (var seedCtx = CreateInMemoryContext(dbName))
        {
            seedCtx.Categories.Add(new Category { Id = categoryId, NameEn = "Test", NameAr = "Test", IsDeleted = false });
            seedCtx.Videos.Add(new Video
            {
                Id = videoId,
                TitleAr = "Video",
                DescriptionAr = "Desc",
                Status = VideoStatus.Published,
                CloudflareStreamId = "cf-stream-id",
                IsDeleted = false
            });
            seedCtx.VideoCategories.Add(new VideoCategory { CategoryId = categoryId, VideoId = videoId });
            await seedCtx.SaveChangesAsync();
        }

        // Act
        using var ctx = CreateInMemoryContext(dbName);
        var handler = new DeleteCategoryCommandHandler(ctx);
        var result = await handler.Handle(new DeleteCategoryCommand { Id = categoryId }, CancellationToken.None);
        await ctx.SaveChangesAsync();

        // Assert — handler succeeded
        Assert.True(result.IsSuccess);

        // Assert — category is SOFT deleted (row still exists, IsDeleted = true)
        using var assertCtx = CreateInMemoryContext(dbName);
        var categoryRow = await assertCtx.Categories
            .IgnoreQueryFilters()  // bypass the global filter to inspect the raw row
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        Assert.NotNull(categoryRow);
        Assert.True(categoryRow.IsDeleted, "Category must be soft-deleted (IsDeleted = true).");

        // Assert — VideoCategory join row is removed
        var joinExists = await assertCtx.VideoCategories.AnyAsync(vc => vc.CategoryId == categoryId);
        Assert.False(joinExists, "VideoCategory join row must be removed after category deletion.");

        // Assert — Video itself is NOT deleted (non-cascade rule)
        var video = await assertCtx.Videos.IgnoreQueryFilters().FirstOrDefaultAsync(v => v.Id == videoId);
        Assert.NotNull(video);
        Assert.False(video.IsDeleted, "Video must NOT be cascade-deleted when its category is deleted.");
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ReturnsFailure_WithResourceNotFound()
    {
        var options = new DbContextOptionsBuilder<MediaPortfolioDbContext>()
            .UseInMemoryDatabase($"DeleteCategoryNotFound_{Guid.NewGuid()}")
            .Options;
        using var ctx = new MediaPortfolioDbContext(options);

        var handler = new DeleteCategoryCommandHandler(ctx);
        var result = await handler.Handle(new DeleteCategoryCommand { Id = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("RESOURCE_NOT_FOUND", result.ErrorCode);
    }
}
