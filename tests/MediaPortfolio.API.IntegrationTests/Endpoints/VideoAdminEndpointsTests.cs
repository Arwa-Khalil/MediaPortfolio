using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MediaPortfolio.API.IntegrationTests.Endpoints;

public class VideoAdminEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ApiWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public VideoAdminEndpointsTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = "admin@sainin.com", password = "TestPassw0rd!" });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
    }

    [Fact]
    public async Task PostVideoUploadUrl_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsync("/api/v1/admin/media/video-upload-url", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostVideoUploadUrl_WithAuth_Returns200()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _client.PostAsync("/api/v1/admin/media/video-upload-url", null);
        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("uploadUrl", body);
    }

    [Fact]
    public async Task PostVideo_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/admin/videos", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostVideo_WithAuth_Returns201()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 1. Create a category to associate
        var catResponse = await _client.PostAsJsonAsync("/api/v1/admin/categories", new { nameEn = $"Cat_{Guid.NewGuid()}", nameAr = $"Cat_{Guid.NewGuid()}" });
        var catDoc = JsonDocument.Parse(await catResponse.Content.ReadAsStringAsync());
        var categoryId = catDoc.RootElement.GetProperty("data").GetString()!;

        // 2. Create the video
        var videoResponse = await _client.PostAsJsonAsync("/api/v1/admin/videos", new
        {
            titleAr = "Test Video",
            descriptionAr = "Desc",
            cloudflareStreamId = "stream-123",
            categoryIds = new[] { categoryId },
            status = 1 // Published
        });

        _client.DefaultRequestHeaders.Authorization = null;
        Assert.Equal(HttpStatusCode.Created, videoResponse.StatusCode);
    }

    [Fact]
    public async Task PostVideo_WithBadCategoryId_Returns400()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var videoResponse = await _client.PostAsJsonAsync("/api/v1/admin/videos", new
        {
            titleAr = "Test Video",
            descriptionAr = "Desc",
            cloudflareStreamId = "stream-123",
            categoryIds = new[] { Guid.NewGuid() }, // Non-existent category
            status = 1
        });

        _client.DefaultRequestHeaders.Authorization = null;
        Assert.Equal(HttpStatusCode.BadRequest, videoResponse.StatusCode);
        var body = await videoResponse.Content.ReadAsStringAsync();
        Assert.Contains("RESOURCE_NOT_FOUND", body);
    }

    [Fact]
    public async Task GetVideos_WithAuth_Returns200_Paginated()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/admin/videos?page=1&pageSize=10");
        _client.DefaultRequestHeaders.Authorization = null;

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(HttpStatusCode.OK == response.StatusCode, $"Expected OK, got {response.StatusCode}. Body: {body}");
        var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.GetProperty("data").ValueKind == JsonValueKind.Array);
    }

    [Fact]
    public async Task GetVideos_WithAuth_WithCloudflareThumbnail_Returns200_AndResolvesThumbnailUrl()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 1. Create a category
        var catResponse = await _client.PostAsJsonAsync("/api/v1/admin/categories", new { nameEn = $"Cat_{Guid.NewGuid()}", nameAr = $"Cat_{Guid.NewGuid()}" });
        var catDoc = JsonDocument.Parse(await catResponse.Content.ReadAsStringAsync());
        var categoryId = catDoc.RootElement.GetProperty("data").GetString()!;

        // 2. Create a video with CloudflareThumbnailImageId (simulated by updating it directly, or creating it if the endpoint allows)
        // Since the POST endpoint doesn't accept CloudflareThumbnailImageId, we use the Application DB context directly to seed it for the test.
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MediaPortfolio.Application.Interfaces.IApplicationDbContext>();
            var video = new MediaPortfolio.Domain.Entities.Video
            {
                TitleAr = "Test Video with Thumbnail",
                CloudflareStreamId = "stream-test-123",
                CloudflareThumbnailImageId = "image-test-456", // This is the key field for the regression test
                Status = MediaPortfolio.Domain.Enums.VideoStatus.Draft
            };
            context.Videos.Add(video);
            await context.SaveChangesAsync(default);
        }

        // 3. Fetch the admin video list
        var response = await _client.GetAsync("/api/v1/admin/videos?page=1&pageSize=10");
        _client.DefaultRequestHeaders.Authorization = null;

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // 4. Verify the response contains the resolved thumbnail URL
        var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("data").EnumerateArray();
        var foundVideo = items.FirstOrDefault(i => i.GetProperty("cloudflareStreamId").GetString() == "stream-test-123");
        
        Assert.NotEqual(JsonValueKind.Undefined, foundVideo.ValueKind);
        var thumbnailUrl = foundVideo.GetProperty("thumbnailUrl").GetString();
        Assert.NotNull(thumbnailUrl);
        Assert.Contains("image-test-456", thumbnailUrl);
    }

    [Fact]
    public async Task PutVideoStatus_WithAuth_Returns200()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var catResponse = await _client.PostAsJsonAsync("/api/v1/admin/categories", new { nameEn = $"Cat_{Guid.NewGuid()}", nameAr = $"Cat_{Guid.NewGuid()}" });
        var catDoc = JsonDocument.Parse(await catResponse.Content.ReadAsStringAsync());
        var categoryId = catDoc.RootElement.GetProperty("data").GetString()!;

        var createResponse = await _client.PostAsJsonAsync("/api/v1/admin/videos", new
        {
            titleAr = "Draft Video",
            descriptionAr = "Desc",
            cloudflareStreamId = "stream-123",
            categoryIds = new[] { categoryId },
            status = 0 // Draft
        });
        var createDoc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var videoId = createDoc.RootElement.GetProperty("data").GetString()!;

        var updateResponse = await _client.PatchAsJsonAsync($"/api/v1/admin/videos/{videoId}/status", new { id = videoId, status = 1 });
        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteVideo_WithAuth_Returns204()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var catResponse = await _client.PostAsJsonAsync("/api/v1/admin/categories", new { nameEn = $"Cat_{Guid.NewGuid()}", nameAr = $"Cat_{Guid.NewGuid()}" });
        var catDoc = JsonDocument.Parse(await catResponse.Content.ReadAsStringAsync());
        var categoryId = catDoc.RootElement.GetProperty("data").GetString()!;

        var createResponse = await _client.PostAsJsonAsync("/api/v1/admin/videos", new
        {
            titleAr = "ToDelete",
            descriptionAr = "Desc",
            cloudflareStreamId = "stream-123",
            categoryIds = new[] { categoryId },
            status = 0
        });
        var createDoc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var videoId = createDoc.RootElement.GetProperty("data").GetString()!;

        var deleteResponse = await _client.DeleteAsync($"/api/v1/admin/videos/{videoId}");
        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
