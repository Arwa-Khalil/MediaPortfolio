namespace MediaPortfolio.Application.DTOs;

/// <summary>
/// Public response shape for GET /site-settings (API Contract §7).
/// All fields are returned; placeholder substitution happens client-side.
/// </summary>
public record SiteSettingsDto
{
    public string? LogoUrl { get; init; }
    public string? SnapchatUrl { get; init; }
    public string? TiktokUrl { get; init; }
    public string? InstagramUrl { get; init; }
    public string? WhatsAppNumber { get; init; }
    public string WhatsAppTemplateAr { get; init; } = string.Empty;
    public string WhatsAppTemplateEn { get; init; } = string.Empty;
}
