using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Settings.Commands.UpdateSiteSettings;

/// <summary>
/// PUT /admin/site-settings — API Contract §7.
/// Logo is updated by passing a cloudflareImageId previously obtained via POST /admin/media/image-upload-url.
/// </summary>
public class UpdateSiteSettingsCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// A Cloudflare Image ID obtained via the /admin/media/image-upload-url broker.
    /// Null means "do not change the existing logo".
    /// </summary>
    public string? CloudflareLogoImageId { get; set; }
    public string? SnapchatUrl { get; set; }
    public string? TiktokUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? WhatsAppTemplateAr { get; set; }
    public string? WhatsAppTemplateEn { get; set; }
}
