using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Settings.Queries.GetSiteSettings;

public class GetSiteSettingsQueryHandler : IRequestHandler<GetSiteSettingsQuery, Result<SiteSettingsDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IThumbnailUrlResolver _thumbnailUrlResolver;

    public GetSiteSettingsQueryHandler(IApplicationDbContext context, IThumbnailUrlResolver thumbnailUrlResolver)
    {
        _context = context;
        _thumbnailUrlResolver = thumbnailUrlResolver;
    }

    public async Task<Result<SiteSettingsDto>> Handle(GetSiteSettingsQuery request, CancellationToken cancellationToken)
    {
        // SiteSettings is guaranteed to have exactly one row (seeded in Phase 0).
        var settings = await _context.SiteSettings.FirstOrDefaultAsync(cancellationToken);

        // If not yet seeded, return safe defaults rather than a 500.
        if (settings == null)
        {
            return Result<SiteSettingsDto>.Success(new SiteSettingsDto());
        }

        var dto = new SiteSettingsDto
        {
            // Logo is stored as a Cloudflare Image ID; resolve it to a delivery URL.
            LogoUrl = _thumbnailUrlResolver.ResolveLogoUrl(settings.LogoImageId),
            SnapchatUrl = settings.SnapchatUrl,
            TiktokUrl = settings.TiktokUrl,
            InstagramUrl = settings.InstagramUrl,
            WhatsAppNumber = settings.WhatsAppNumber,
            WhatsAppTemplateAr = settings.WhatsAppTemplateAr,
            WhatsAppTemplateEn = settings.WhatsAppTemplateEn
        };

        return Result<SiteSettingsDto>.Success(dto);
    }
}
