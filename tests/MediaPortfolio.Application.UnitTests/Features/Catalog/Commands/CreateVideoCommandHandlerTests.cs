using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MediaPortfolio.Application.Features.Catalog.Commands.CreateVideo;
using MediaPortfolio.Domain.Entities;
using MediaPortfolio.Domain.Enums;
using MediaPortfolio.Infrastructure.Persistence;

namespace MediaPortfolio.Application.UnitTests.Features.Catalog.Commands;

public class CreateVideoCommandHandlerTests
{
    private static MediaPortfolioDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<MediaPortfolioDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new MediaPortfolioDbContext(options);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesVideo()
    {
        var dbName = $"CreateVideo_{Guid.NewGuid()}";
        var categoryId = Guid.NewGuid();

        using (var seedCtx = CreateInMemoryContext(dbName))
        {
            seedCtx.Categories.Add(new Category { Id = categoryId, NameEn = "Cat1", NameAr = "Cat1", IsDeleted = false });
            await seedCtx.SaveChangesAsync();
        }

        using var ctx = CreateInMemoryContext(dbName);
        var handler = new CreateVideoCommandHandler(ctx);
        var command = new CreateVideoCommand
        {
            TitleAr = "New Video",
            DescriptionAr = "Desc",
            CategoryIds = new List<Guid> { categoryId },
            CloudflareStreamId = "stream-123",
            Status = VideoStatus.Published
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        var video = await ctx.Videos.Include(v => v.VideoCategories).FirstOrDefaultAsync(v => v.Id == result.Value);
        Assert.NotNull(video);
        Assert.Equal("New Video", video.TitleAr);
        Assert.NotNull(video.PublishedAt);
        Assert.Single(video.VideoCategories);
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ReturnsFailure()
    {
        var dbName = $"CreateVideo_NoCat_{Guid.NewGuid()}";
        using var ctx = CreateInMemoryContext(dbName);
        var handler = new CreateVideoCommandHandler(ctx);

        var command = new CreateVideoCommand
        {
            TitleAr = "New Video",
            DescriptionAr = "Desc",
            CategoryIds = new List<Guid> { Guid.NewGuid() },
            CloudflareStreamId = "stream-123",
            Status = VideoStatus.Published
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("RESOURCE_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task Handle_SoftDeletedCategory_ReturnsFailure()
    {
        var dbName = $"CreateVideo_SoftDelCat_{Guid.NewGuid()}";
        var categoryId = Guid.NewGuid();

        using (var seedCtx = CreateInMemoryContext(dbName))
        {
            // Category is soft-deleted
            seedCtx.Categories.Add(new Category { Id = categoryId, NameEn = "Cat1", NameAr = "Cat1", IsDeleted = true });
            await seedCtx.SaveChangesAsync();
        }

        using var ctx = CreateInMemoryContext(dbName);
        var handler = new CreateVideoCommandHandler(ctx);

        var command = new CreateVideoCommand
        {
            TitleAr = "New Video",
            DescriptionAr = "Desc",
            CategoryIds = new List<Guid> { categoryId },
            CloudflareStreamId = "stream-123",
            Status = VideoStatus.Published
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("RESOURCE_NOT_FOUND", result.ErrorCode);
    }
}
