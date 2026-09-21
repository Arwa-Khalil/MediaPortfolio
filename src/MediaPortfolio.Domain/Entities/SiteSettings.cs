namespace MediaPortfolio.Domain.Entities;

public class SiteSettings
{
    public int Id { get; set; } = 1; // Single row
    public string? LogoImageId { get; set; }
    public string? SnapchatUrl { get; set; }
    public string? TiktokUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string WhatsAppTemplateAr { get; set; } = string.Empty;
    public string WhatsAppTemplateEn { get; set; } = string.Empty;
}
