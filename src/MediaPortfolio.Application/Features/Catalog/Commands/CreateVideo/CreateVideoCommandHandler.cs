using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Domain.Entities;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Application.Features.Catalog.Commands.CreateVideo;

public class CreateVideoCommandHandler : IRequestHandler<CreateVideoCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateVideoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateVideoCommand request, CancellationToken cancellationToken)
    {
        // Note: the Global Query Filter automatically excludes soft-deleted Categories
        var existingCategoryIds = await _context.Categories
            .Where(c => request.CategoryIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var missingCategoryIds = request.CategoryIds.Except(existingCategoryIds).ToList();
        if (missingCategoryIds.Any())
        {
            return Result<Guid>.Failure("RESOURCE_NOT_FOUND", $"One or more categories were not found: {string.Join(", ", missingCategoryIds)}");
        }

        var video = new Video
        {
            Id = Guid.NewGuid(),
            TitleAr = request.TitleAr,
            DescriptionAr = request.DescriptionAr,
            CloudflareStreamId = request.CloudflareStreamId,
            CloudflareThumbnailImageId = request.CloudflareThumbnailImageId,
            Status = request.Status,
            PublishedAt = request.Status == VideoStatus.Published ? DateTimeOffset.UtcNow : null
        };

        foreach (var categoryId in request.CategoryIds)
        {
            video.VideoCategories.Add(new VideoCategory
            {
                VideoId = video.Id,
                CategoryId = categoryId
            });
        }

        _context.Videos.Add(video);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(video.Id);
    }
}
