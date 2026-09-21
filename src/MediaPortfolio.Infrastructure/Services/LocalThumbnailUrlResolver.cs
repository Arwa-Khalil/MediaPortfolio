using Microsoft.AspNetCore.Http;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Infrastructure.Services;

public class LocalThumbnailUrlResolver : IThumbnailUrlResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalThumbnailUrlResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? ResolveVideoThumbnailUrl(string? cloudflareThumbnailImageId)
    {
        var baseUrl = GetBaseUrl();

        if (string.IsNullOrWhiteSpace(cloudflareThumbnailImageId))
        {
            return $"{baseUrl}/local-media/placeholder-thumbnail.png";
        }

        return $"{baseUrl}/local-media/image/{cloudflareThumbnailImageId}";
    }

    public string? ResolveLogoUrl(string? logoImageId)
    {
        if (string.IsNullOrWhiteSpace(logoImageId))
        {
            return null;
        }

        return $"{GetBaseUrl()}/local-media/image/{logoImageId}";
    }

    private string GetBaseUrl()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        return request != null
            ? $"{request.Scheme}://{request.Host}"
            : "http://localhost:5079";
    }
}

