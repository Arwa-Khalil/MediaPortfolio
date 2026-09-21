using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MediaPortfolio.API.IntegrationTests.Endpoints;

public class AnalyticsEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly System.Net.Http.HttpClient _client;

    public AnalyticsEndpointsTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = "admin@sainin.com", password = "TestPassw0rd!" });
        response.EnsureSuccessStatusCode();
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
    }

    [Fact]
    public async Task GetVideoAnalytics_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/admin/analytics/videos");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetVideoAnalytics_WithAuth_Returns200()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/admin/analytics/videos");
        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        
        Assert.True(doc.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(JsonValueKind.Array, doc.RootElement.GetProperty("data").ValueKind);
    }
}
