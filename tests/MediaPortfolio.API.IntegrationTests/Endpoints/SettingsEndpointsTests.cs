using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using MediaPortfolio.Application.DTOs;

namespace MediaPortfolio.API.IntegrationTests.Endpoints;

public class SettingsEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly System.Net.Http.HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public SettingsEndpointsTests(ApiWebApplicationFactory factory)
    {
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
    public async Task GetSiteSettings_Public_Returns200_WithResolvedLanguage()
    {
        var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, "/api/v1/site-settings");
        request.Headers.Add("Accept-Language", "ar");
        
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var data = doc.RootElement.GetProperty("data");
        
        Assert.True(data.TryGetProperty("whatsAppTemplateAr", out _));
        Assert.True(data.TryGetProperty("whatsAppTemplateEn", out _));
    }

    [Fact]
    public async Task PutSiteSettings_WithoutAuth_Returns401()
    {
        var response = await _client.PutAsJsonAsync("/api/v1/admin/site-settings", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PutSiteSettings_WithAuth_UpdatesFields_AndCanBeFetched()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var newSnapchat = "https://snapchat.com/add/updated";
        var updateRes = await _client.PutAsJsonAsync("/api/v1/admin/site-settings", new
        {
            snapchatUrl = newSnapchat
        });

        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;

        var getRes = await _client.GetAsync("/api/v1/site-settings");
        getRes.EnsureSuccessStatusCode();
        var doc = JsonDocument.Parse(await getRes.Content.ReadAsStringAsync());
        
        Assert.Equal(newSnapchat, doc.RootElement.GetProperty("data").GetProperty("snapchatUrl").GetString());
    }
}
