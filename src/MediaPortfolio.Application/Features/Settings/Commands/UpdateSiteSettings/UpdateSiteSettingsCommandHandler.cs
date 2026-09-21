using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Settings.Commands.UpdateSiteSettings;

public class UpdateSiteSettingsCommandHandler : IRequestHandler<UpdateSiteSettingsCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public UpdateSiteSettingsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateSiteSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _context.SiteSettings.FirstOrDefaultAsync(cancellationToken);

        if (settings == null)
        {
            // Seed a new one if it somehow doesn't exist.
            settings = new Domain.Entities.SiteSettings();
            _context.SiteSettings.Add(settings);
        }

        if (request.CloudflareLogoImageId != null)
        {
            settings.LogoImageId = request.CloudflareLogoImageId;
        }

        if (request.SnapchatUrl != null)
        {
            settings.SnapchatUrl = string.IsNullOrWhiteSpace(request.SnapchatUrl) ? null : request.SnapchatUrl;
        }

        if (request.TiktokUrl != null)
        {
            settings.TiktokUrl = string.IsNullOrWhiteSpace(request.TiktokUrl) ? null : request.TiktokUrl;
        }

        if (request.InstagramUrl != null)
        {
            settings.InstagramUrl = string.IsNullOrWhiteSpace(request.InstagramUrl) ? null : request.InstagramUrl;
        }

        if (request.WhatsAppNumber != null)
        {
            settings.WhatsAppNumber = request.WhatsAppNumber;
        }

        if (request.WhatsAppTemplateAr != null)
        {
            settings.WhatsAppTemplateAr = request.WhatsAppTemplateAr;
        }

        if (request.WhatsAppTemplateEn != null)
        {
            settings.WhatsAppTemplateEn = request.WhatsAppTemplateEn;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
