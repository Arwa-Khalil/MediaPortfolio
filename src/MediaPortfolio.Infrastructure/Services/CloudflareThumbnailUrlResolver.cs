using Microsoft.Extensions.Configuration;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Infrastructure.Services;

public class CloudflareThumbnailUrlResolver : IThumbnailUrlResolver
{
    private readonly IConfiguration _configuration;

    public CloudflareThumbnailUrlResolver(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? ResolveVideoThumbnailUrl(string? cloudflareThumbnailImageId)
    {
        if (string.IsNullOrWhiteSpace(cloudflareThumbnailImageId))
        {
            return null;
        }

        var accountHash = _configuration["Cloudflare:ImagesAccountHash"];
        if (string.IsNullOrEmpty(accountHash))
        {
            return null;
        }

        return $"https://imagedelivery.net/{accountHash}/{cloudflareThumbnailImageId}/public";
    }

    public string? ResolveLogoUrl(string? logoImageId)
    {
        if (string.IsNullOrWhiteSpace(logoImageId))
        {
            return null;
        }

        var accountHash = _configuration["Cloudflare:ImagesAccountHash"];
        if (string.IsNullOrEmpty(accountHash))
        {
            return null;
        }

        return $"https://imagedelivery.net/{accountHash}/{logoImageId}/public";
    }
}

