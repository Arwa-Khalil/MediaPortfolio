using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MediaPortfolio.API.IntegrationTests.Endpoints;

public class AdminAuthAuditTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AdminAuthAuditTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    // Categories
    [InlineData("POST", "/api/v1/admin/categories")]
    [InlineData("PUT", "/api/v1/admin/categories/00000000-0000-0000-0000-000000000000")]
    [InlineData("DELETE", "/api/v1/admin/categories/00000000-0000-0000-0000-000000000000")]
    // Videos
    [InlineData("POST", "/api/v1/admin/videos")]
    [InlineData("PUT", "/api/v1/admin/videos/00000000-0000-0000-0000-000000000000")]
    [InlineData("PATCH", "/api/v1/admin/videos/00000000-0000-0000-0000-000000000000/status")]
    [InlineData("DELETE", "/api/v1/admin/videos/00000000-0000-0000-0000-000000000000")]
    [InlineData("GET", "/api/v1/admin/videos")]
    // Secondary Services
    [InlineData("POST", "/api/v1/admin/secondary-services")]
    [InlineData("PUT", "/api/v1/admin/secondary-services/00000000-0000-0000-0000-000000000000")]
    [InlineData("DELETE", "/api/v1/admin/secondary-services/00000000-0000-0000-0000-000000000000")]
    // Media Upload
    [InlineData("POST", "/api/v1/admin/media/video-upload-url")]
    [InlineData("POST", "/api/v1/admin/media/image-upload-url")]
    // Contact Submissions
    [InlineData("GET", "/api/v1/admin/contact-submissions")]
    [InlineData("PATCH", "/api/v1/admin/contact-submissions/00000000-0000-0000-0000-000000000000/mark-read")]
    [InlineData("DELETE", "/api/v1/admin/contact-submissions/00000000-0000-0000-0000-000000000000")]
    // Site Settings
    [InlineData("PUT", "/api/v1/admin/site-settings")]
    // Analytics
    [InlineData("GET", "/api/v1/admin/analytics/videos")]
    // Auth (Admin actions) — logout now requires Bearer token per contract
    [InlineData("POST", "/api/v1/auth/logout")]
    [InlineData("POST", "/api/v1/auth/change-password")]
    public async Task AllAdminEndpoints_WithoutAuth_Return401(string method, string url)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method == "POST" || method == "PUT" || method == "PATCH")
        {
            request.Content = new System.Net.Http.StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        }

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
