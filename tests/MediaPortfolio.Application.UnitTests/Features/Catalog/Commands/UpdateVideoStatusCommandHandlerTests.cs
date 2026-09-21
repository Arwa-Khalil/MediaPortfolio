using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using MediaPortfolio.Application.Features.Catalog.Commands.UpdateVideoStatus;
using MediaPortfolio.Domain.Entities;
using MediaPortfolio.Domain.Enums;
using MediaPortfolio.Infrastructure.Persistence;

namespace MediaPortfolio.Application.UnitTests.Features.Catalog.Commands;

public class UpdateVideoStatusCommandHandlerTests
{
    private static MediaPortfolioDbContext CreateInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<MediaPortfolioDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new MediaPortfolioDbContext(options);
    }

    [Fact]
    public async Task Handle_DraftToPublished_SetsPublishedAt()
    {
        var dbName = $"UpdateStatus_{Guid.NewGuid()}";
        var videoId = Guid.NewGuid();

        using (var seedCtx = CreateInMemoryContext(dbName))
        {
            seedCtx.Videos.Add(new Video
            {
                Id = videoId,
                TitleAr = "Draft Video",
                DescriptionAr = "Desc",
                Status = VideoStatus.Draft,
                PublishedAt = null,
                CloudflareStreamId = "stream-123"
            });
            await seedCtx.SaveChangesAsync();
        }

        using var ctx = CreateInMemoryContext(dbName);
        var handler = new UpdateVideoStatusCommandHandler(ctx);
        var command = new UpdateVideoStatusCommand { Id = videoId, Status = VideoStatus.Published };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var video = await ctx.Videos.FindAsync(videoId);
        Assert.NotNull(video);
        Assert.NotNull(video.PublishedAt);
        Assert.Equal(VideoStatus.Published, video.Status);
    }

    [Fact]
    public async Task Handle_PublishedToDraft_ClearsPublishedAt()
    {
        var dbName = $"UpdateStatus_{Guid.NewGuid()}";
        var videoId = Guid.NewGuid();

        using (var seedCtx = CreateInMemoryContext(dbName))
        {
            seedCtx.Videos.Add(new Video
            {
                Id = videoId,
                TitleAr = "Pub Video",
                DescriptionAr = "Desc",
                Status = VideoStatus.Published,
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-1),
                CloudflareStreamId = "stream-123"
            });
            await seedCtx.SaveChangesAsync();
        }

        using var ctx = CreateInMemoryContext(dbName);
        var handler = new UpdateVideoStatusCommandHandler(ctx);
        var command = new UpdateVideoStatusCommand { Id = videoId, Status = VideoStatus.Draft };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var video = await ctx.Videos.FindAsync(videoId);
        Assert.NotNull(video);
        Assert.Null(video.PublishedAt);
        Assert.Equal(VideoStatus.Draft, video.Status);
    }
}
