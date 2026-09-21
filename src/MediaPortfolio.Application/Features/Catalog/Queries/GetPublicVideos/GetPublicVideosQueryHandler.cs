using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Application.Features.Catalog.Queries.GetPublicVideos;

public class GetPublicVideosQueryHandler : IRequestHandler<GetPublicVideosQuery, Result<PagedResult<PublicVideoDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IThumbnailUrlResolver _thumbnailUrlResolver;

    public GetPublicVideosQueryHandler(
        IApplicationDbContext context,
        IThumbnailUrlResolver thumbnailUrlResolver)
    {
        _context = context;
        _thumbnailUrlResolver = thumbnailUrlResolver;
    }

    public async Task<Result<PagedResult<PublicVideoDto>>> Handle(
        GetPublicVideosQuery request,
        CancellationToken cancellationToken)
    {
        // Base filter: published, not soft-deleted.
        // The EF Global Query Filter for IsDeleted is applied automatically on Videos.
        var query = _context.Videos
            .Include(v => v.VideoCategories)
                .ThenInclude(vc => vc.Category)
            .Where(v => v.Status == VideoStatus.Published && !v.IsDeleted)
            .AsQueryable();

        // Optional category filter — join through VideoCategories.
        if (request.CategoryId.HasValue)
        {
            var catId = request.CategoryId.Value;
            query = query.Where(v => v.VideoCategories.Any(vc => vc.CategoryId == catId));
        }

        query = query.OrderByDescending(v => v.PublishedAt).ThenBy(v => v.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 20;

        var videoEntities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Projection happens in-memory after fetch to use the resolver.
        // TitleAr / DescriptionAr are deliberately NOT projected — they must not
        // appear in PublicVideoDto at all (not even as null keys on the wire).
        var dtos = videoEntities
            .Select(v => new PublicVideoDto
            {
                Id = v.Id,
                ThumbnailUrl = _thumbnailUrlResolver.ResolveVideoThumbnailUrl(v.CloudflareThumbnailImageId),
                CloudflareStreamId = v.CloudflareStreamId,
                Categories = v.VideoCategories
                    .Where(vc => vc.Category != null)
                    .Select(vc => new VideoCategoryDto
                    {
                        Id = vc.Category!.Id,
                        NameEn = vc.Category.NameEn,
                        NameAr = vc.Category.NameAr
                    }).ToList()
            })
            .ToList();

        return Result<PagedResult<PublicVideoDto>>.Success(new PagedResult<PublicVideoDto>
        {
            Items = dtos,
            TotalCount = totalCount
        });
    }
}
