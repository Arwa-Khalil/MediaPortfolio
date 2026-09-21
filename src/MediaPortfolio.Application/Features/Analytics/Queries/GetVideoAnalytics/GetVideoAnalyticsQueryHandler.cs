using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Analytics.Queries.GetVideoAnalytics;

public class GetVideoAnalyticsQueryHandler : IRequestHandler<GetVideoAnalyticsQuery, Result<IEnumerable<AdminVideoDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IThumbnailUrlResolver _thumbnailUrlResolver;

    public GetVideoAnalyticsQueryHandler(IApplicationDbContext context, IThumbnailUrlResolver thumbnailUrlResolver)
    {
        _context = context;
        _thumbnailUrlResolver = thumbnailUrlResolver;
    }

    public async Task<Result<IEnumerable<AdminVideoDto>>> Handle(GetVideoAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var videos = await _context.Videos
            .Include(v => v.VideoCategories)
                .ThenInclude(vc => vc.Category)
            .OrderByDescending(v => v.ViewCount)
            .ThenByDescending(v => v.WhatsAppClickCount)
            .ToListAsync(cancellationToken);

        var dtos = videos.Select(v => new AdminVideoDto
        {
            Id = v.Id,
            TitleAr = v.TitleAr,
            DescriptionAr = v.DescriptionAr,
            CloudflareStreamId = v.CloudflareStreamId,
            CloudflareThumbnailImageId = v.CloudflareThumbnailImageId,
            ThumbnailUrl = _thumbnailUrlResolver.ResolveVideoThumbnailUrl(v.CloudflareThumbnailImageId),
            Status = v.Status,
            PublishedAt = v.PublishedAt,
            ViewCount = v.ViewCount,
            WhatsAppClickCount = v.WhatsAppClickCount,
            FormSubmissionCount = v.FormSubmissionCount,
            Categories = v.VideoCategories
                .Where(vc => vc.Category != null)
                .Select(vc => new VideoCategoryDto
                {
                    Id = vc.Category!.Id,
                    NameEn = vc.Category.NameEn,
                    NameAr = vc.Category.NameAr
                }).ToList()
        }).ToList();

        return Result<IEnumerable<AdminVideoDto>>.Success(dtos);
    }
}
