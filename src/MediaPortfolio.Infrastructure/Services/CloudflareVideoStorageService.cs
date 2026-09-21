using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Infrastructure.Services;

public class CloudflareVideoStorageService : IVideoStorageService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CloudflareVideoStorageService> _logger;

    public CloudflareVideoStorageService(HttpClient httpClient, IConfiguration configuration, ILogger<CloudflareVideoStorageService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<MediaUploadUrlResult> GetUploadUrlAsync()
    {
        var accountId = _configuration["Cloudflare:StreamAccountId"];
        var apiToken = _configuration["Cloudflare:StreamApiToken"];

        if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(apiToken))
        {
            throw new InvalidOperationException("Cloudflare Stream credentials are not configured.");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.cloudflare.com/client/v4/accounts/{accountId}/stream/direct_upload");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        
        request.Content = JsonContent.Create(new { maxDurationSeconds = 36000 }); // 10 hours max

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(responseBody);
        
        var resultElement = document.RootElement.GetProperty("result");
        var uploadUrl = resultElement.GetProperty("uploadURL").GetString();
        var uid = resultElement.GetProperty("uid").GetString();

        if (string.IsNullOrEmpty(uploadUrl) || string.IsNullOrEmpty(uid))
        {
            throw new InvalidOperationException("Failed to parse upload URL or UID from Cloudflare Stream response.");
        }

        return new MediaUploadUrlResult(uploadUrl, uid);
    }

    public async Task DeleteVideoAsync(string cloudflareStreamId)
    {
        var accountId = _configuration["Cloudflare:StreamAccountId"];
        var apiToken = _configuration["Cloudflare:StreamApiToken"];

        if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(apiToken))
        {
            throw new InvalidOperationException("Cloudflare Stream credentials are not configured.");
        }

        var request = new HttpRequestMessage(HttpMethod.Delete, $"https://api.cloudflare.com/client/v4/accounts/{accountId}/stream/{cloudflareStreamId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to delete video {VideoId} from Cloudflare Stream. Status Code: {StatusCode}", cloudflareStreamId, response.StatusCode);
        }
    }
}
