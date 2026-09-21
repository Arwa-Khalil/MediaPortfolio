using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Application.Features.Catalog.Queries.SearchPublicVideos;

public class SearchPublicVideosQueryHandler : IRequestHandler<SearchPublicVideosQuery, Result<PagedResult<PublicVideoDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IThumbnailUrlResolver _thumbnailUrlResolver;

    public SearchPublicVideosQueryHandler(
        IApplicationDbContext context,
        IThumbnailUrlResolver thumbnailUrlResolver)
    {
        _context = context;
        _thumbnailUrlResolver = thumbnailUrlResolver;
    }

    public async Task<Result<PagedResult<PublicVideoDto>>> Handle(
        SearchPublicVideosQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Videos
            .Include(v => v.VideoCategories)
                .ThenInclude(vc => vc.Category)
            .Where(v => v.Status == VideoStatus.Published && !v.IsDeleted)
            .AsQueryable();

        // Per Requirements §4.3: search matches against both TitleAr and DescriptionAr,
        // case-insensitive, partial match. Npgsql translates ToLower().Contains to ILIKE or similar.
        // The matched fields are NEVER projected — they power the WHERE clause only.
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var term = request.Q.ToLower();
            query = query.Where(v =>
                (v.TitleAr != null && v.TitleAr.ToLower().Contains(term)) ||
                (v.DescriptionAr != null && v.DescriptionAr.ToLower().Contains(term)));
        }

        query = query.OrderByDescending(v => v.PublishedAt).ThenBy(v => v.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 20;

        var videoEntities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

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
