using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Contact.Queries.GetAdminContactSubmissions;

public class GetAdminContactSubmissionsQueryHandler : IRequestHandler<GetAdminContactSubmissionsQuery, Result<PagedResult<ContactSubmissionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IThumbnailUrlResolver _thumbnailUrlResolver;

    public GetAdminContactSubmissionsQueryHandler(
        IApplicationDbContext context, 
        IThumbnailUrlResolver thumbnailUrlResolver)
    {
        _context = context;
        _thumbnailUrlResolver = thumbnailUrlResolver;
    }

    public async Task<Result<PagedResult<ContactSubmissionDto>>> Handle(GetAdminContactSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 20;

        var query = _context.ContactSubmissions.AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(cs => cs.Status == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var submissions = await query
            .OrderByDescending(cs => cs.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Fetch origins separately to avoid complex joins across optional nullable fields
        var categoryIds = submissions.Where(s => s.OriginCategoryId.HasValue).Select(s => s.OriginCategoryId!.Value).Distinct().ToList();
        var videoIds = submissions.Where(s => s.OriginVideoId.HasValue).Select(s => s.OriginVideoId!.Value).Distinct().ToList();

        var categories = await _context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, cancellationToken);
            
        var videos = await _context.Videos
            .Where(v => videoIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        var dtos = submissions.Select(s => new ContactSubmissionDto
        {
            Id = s.Id,
            Name = s.Name,
            Phone = s.Phone,
            Email = s.Email,
            Message = s.Message,
            Status = s.Status,
            CreatedAt = s.CreatedAt,
            ReadAt = s.ReadAt,
            OriginCategory = s.OriginCategoryId.HasValue && categories.TryGetValue(s.OriginCategoryId.Value, out var cat) 
                ? new VideoCategoryDto { Id = cat.Id, NameEn = cat.NameEn, NameAr = cat.NameAr } 
                : null,
            OriginVideo = s.OriginVideoId.HasValue && videos.TryGetValue(s.OriginVideoId.Value, out var vid) 
                ? new OriginVideoSummaryDto { Id = vid.Id, ThumbnailUrl = _thumbnailUrlResolver.ResolveVideoThumbnailUrl(vid.CloudflareThumbnailImageId) } 
                : null
        }).ToList();

        return Result<PagedResult<ContactSubmissionDto>>.Success(new PagedResult<ContactSubmissionDto>
        {
            Items = dtos,
            TotalCount = totalCount
        });
    }
}
