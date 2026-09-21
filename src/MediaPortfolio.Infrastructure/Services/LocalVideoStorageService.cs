using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Infrastructure.Services;

public class LocalVideoStorageService : IVideoStorageService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalVideoStorageService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<MediaUploadUrlResult> GetUploadUrlAsync()
    {
        var assetId = Guid.NewGuid().ToString("N");
        var request = _httpContextAccessor.HttpContext?.Request;
        
        var baseUrl = request != null 
            ? $"{request.Scheme}://{request.Host}" 
            : "http://localhost:5079";

        var uploadUrl = $"{baseUrl}/local-media/upload/video/{assetId}";
        
        return Task.FromResult(new MediaUploadUrlResult(uploadUrl, assetId));
    }

    public Task DeleteVideoAsync(string cloudflareStreamId)
    {
        var filePath = Path.Combine("/media-storage", "videos", cloudflareStreamId);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        return Task.CompletedTask;
    }
}
