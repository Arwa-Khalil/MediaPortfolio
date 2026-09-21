namespace MediaPortfolio.Application.Interfaces;

public interface IThumbnailUrlResolver
{
    string? ResolveVideoThumbnailUrl(string? cloudflareThumbnailImageId);

    /// <summary>
    /// Resolves a Cloudflare Image ID stored in SiteSettings.LogoImageId to a delivery URL.
    /// Uses the same account and delivery pattern as thumbnails.
    /// </summary>
    string? ResolveLogoUrl(string? logoImageId);
}
