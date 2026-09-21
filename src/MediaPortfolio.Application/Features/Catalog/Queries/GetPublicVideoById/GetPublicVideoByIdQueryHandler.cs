using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Application.Features.Catalog.Queries.GetPublicVideoById;

public class GetPublicVideoByIdQueryHandler : IRequestHandler<GetPublicVideoByIdQuery, Result<PublicVideoDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IThumbnailUrlResolver _thumbnailUrlResolver;

    public GetPublicVideoByIdQueryHandler(
        IApplicationDbContext context,
        IThumbnailUrlResolver thumbnailUrlResolver)
    {
        _context = context;
        _thumbnailUrlResolver = thumbnailUrlResolver;
    }

    public async Task<Result<PublicVideoDto>> Handle(
        GetPublicVideoByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Single filter: Id matches AND Published AND not deleted.
        // If the video is Draft, soft-deleted, or doesn't exist at all, this returns null —
        // one code path, so the caller cannot distinguish between "draft" and "missing".
        // This is the anti-leak requirement: same RESOURCE_NOT_FOUND in all three cases.
        var video = await _context.Videos
            .Include(v => v.VideoCategories)
                .ThenInclude(vc => vc.Category)
            .Where(v => v.Id == request.Id
                        && v.Status == VideoStatus.Published
                        && !v.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (video is null)
        {
            return Result<PublicVideoDto>.Failure("RESOURCE_NOT_FOUND", "Video not found.");
        }

        var dto = new PublicVideoDto
        {
            Id = video.Id,
            ThumbnailUrl = _thumbnailUrlResolver.ResolveVideoThumbnailUrl(video.CloudflareThumbnailImageId),
            CloudflareStreamId = video.CloudflareStreamId,
            Categories = video.VideoCategories
                .Where(vc => vc.Category != null)
                .Select(vc => new VideoCategoryDto
                {
                    Id = vc.Category!.Id,
                    NameEn = vc.Category.NameEn,
                    NameAr = vc.Category.NameAr
                }).ToList()
        };

        return Result<PublicVideoDto>.Success(dto);
    }
}
