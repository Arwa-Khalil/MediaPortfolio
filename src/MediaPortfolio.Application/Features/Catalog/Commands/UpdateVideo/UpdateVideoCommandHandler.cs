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

namespace MediaPortfolio.Application.Features.Catalog.Commands.UpdateVideo;

public class UpdateVideoCommandHandler : IRequestHandler<UpdateVideoCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public UpdateVideoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(UpdateVideoCommand request, CancellationToken cancellationToken)
    {
        var video = await _context.Videos
            .Include(v => v.VideoCategories)
            .FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (video == null)
        {
            return Result<Guid>.Failure("RESOURCE_NOT_FOUND", "Video not found.");
        }

        var existingCategoryIds = await _context.Categories
            .Where(c => request.CategoryIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var missingCategoryIds = request.CategoryIds.Except(existingCategoryIds).ToList();
        if (missingCategoryIds.Any())
        {
            return Result<Guid>.Failure("RESOURCE_NOT_FOUND", $"One or more categories were not found: {string.Join(", ", missingCategoryIds)}");
        }

        video.TitleAr = request.TitleAr;
        video.DescriptionAr = request.DescriptionAr;
        video.CloudflareStreamId = request.CloudflareStreamId;
        video.CloudflareThumbnailImageId = request.CloudflareThumbnailImageId;

        if (video.Status != request.Status)
        {
            video.Status = request.Status;
            video.PublishedAt = request.Status == VideoStatus.Published ? DateTimeOffset.UtcNow : null;
        }

        var currentCategoryIds = video.VideoCategories.Select(vc => vc.CategoryId).ToList();
        
        var toRemove = video.VideoCategories.Where(vc => !request.CategoryIds.Contains(vc.CategoryId)).ToList();
        foreach (var rc in toRemove)
        {
            video.VideoCategories.Remove(rc);
        }

        var toAdd = request.CategoryIds.Except(currentCategoryIds).ToList();
        foreach (var ac in toAdd)
        {
            video.VideoCategories.Add(new VideoCategory
            {
                VideoId = video.Id,
                CategoryId = ac
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(video.Id);
    }
}
