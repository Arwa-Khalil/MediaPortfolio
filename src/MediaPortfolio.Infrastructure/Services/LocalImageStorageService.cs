using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Infrastructure.Services;

public class LocalImageStorageService : IImageStorageService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalImageStorageService(IHttpContextAccessor httpContextAccessor)
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

        var uploadUrl = $"{baseUrl}/local-media/upload/image/{assetId}";
        
        return Task.FromResult(new MediaUploadUrlResult(uploadUrl, assetId));
    }

    public Task DeleteImageAsync(string cloudflareImageId)
    {
        var filePath = Path.Combine("/media-storage", "images", cloudflareImageId);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        return Task.CompletedTask;
    }
}
