using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Catalog.Queries.GetAdminVideos;

public class GetAdminVideosQueryHandler : IRequestHandler<GetAdminVideosQuery, Result<PagedResult<AdminVideoDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IThumbnailUrlResolver _thumbnailUrlResolver;

    public GetAdminVideosQueryHandler(IApplicationDbContext context, IThumbnailUrlResolver thumbnailUrlResolver)
    {
        _context = context;
        _thumbnailUrlResolver = thumbnailUrlResolver;
    }

    public async Task<Result<PagedResult<AdminVideoDto>>> Handle(GetAdminVideosQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 20;

        var query = _context.Videos
            .Include(v => v.VideoCategories)
                .ThenInclude(vc => vc.Category)
            .OrderByDescending(v => v.CreatedAt)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var videoEntities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = videoEntities
            .Select(v => new AdminVideoDto
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
                    .Where(vc => vc.Category != null) // Safety check
                    .Select(vc => new VideoCategoryDto
                    {
                        Id = vc.Category!.Id,
                        NameEn = vc.Category.NameEn,
                        NameAr = vc.Category.NameAr
                    }).ToList()
            })
            .ToList();

        var result = new PagedResult<AdminVideoDto>
        {
            Items = dtos,
            TotalCount = totalCount
        };

        return Result<PagedResult<AdminVideoDto>>.Success(result);
    }
}
