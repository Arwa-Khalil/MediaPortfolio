using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Infrastructure.Services;

public class CloudflareImageStorageService : IImageStorageService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CloudflareImageStorageService> _logger;

    public CloudflareImageStorageService(HttpClient httpClient, IConfiguration configuration, ILogger<CloudflareImageStorageService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<MediaUploadUrlResult> GetUploadUrlAsync()
    {
        var accountId = _configuration["Cloudflare:ImagesAccountId"];
        var apiToken = _configuration["Cloudflare:ImagesApiToken"];

        if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(apiToken))
        {
            throw new InvalidOperationException("Cloudflare Images credentials are not configured.");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.cloudflare.com/client/v4/accounts/{accountId}/images/v2/direct_upload");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        
        var content = new MultipartFormDataContent();
        content.Add(new StringContent("true"), "requireSignedURLs");
        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var document = JsonDocument.Parse(responseBody);
        
        var resultElement = document.RootElement.GetProperty("result");
        var uploadUrl = resultElement.GetProperty("uploadURL").GetString();
        var id = resultElement.GetProperty("id").GetString();

        if (string.IsNullOrEmpty(uploadUrl) || string.IsNullOrEmpty(id))
        {
            throw new InvalidOperationException("Failed to parse upload URL or ID from Cloudflare Images response.");
        }

        return new MediaUploadUrlResult(uploadUrl, id);
    }

    public async Task DeleteImageAsync(string cloudflareImageId)
    {
        var accountId = _configuration["Cloudflare:ImagesAccountId"];
        var apiToken = _configuration["Cloudflare:ImagesApiToken"];

        if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(apiToken))
        {
            throw new InvalidOperationException("Cloudflare Images credentials are not configured.");
        }

        var request = new HttpRequestMessage(HttpMethod.Delete, $"https://api.cloudflare.com/client/v4/accounts/{accountId}/images/v1/{cloudflareImageId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to delete image {ImageId} from Cloudflare Images. Status Code: {StatusCode}", cloudflareImageId, response.StatusCode);
        }
    }
}
