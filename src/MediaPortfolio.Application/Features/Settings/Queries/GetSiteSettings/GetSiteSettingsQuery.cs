using MediatR;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;

namespace MediaPortfolio.Application.Features.Settings.Queries.GetSiteSettings;

public class GetSiteSettingsQuery : IRequest<Result<SiteSettingsDto>>
{
    /// <summary>
    /// The language requested via Accept-Language header (e.g. "ar" or "en").
    /// Used to select the appropriate WhatsApp template in the response.
    /// Per API Contract §7: both template strings are returned to the client;
    /// the client performs placeholder substitution. This field is preserved for
    /// future Accept-Language-driven filtering if the contract evolves.
    /// </summary>
    public string Language { get; set; } = "ar";
}
