using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MediaPortfolio.API.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for Secondary Service endpoints (Admin + Public).
/// Tests run against a real PostgreSQL Testcontainers database.
/// </summary>
public class SecondaryServiceEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SecondaryServiceEndpointsTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public class AuthResult
    {
        public AuthTokensDto Data { get; set; } = default!;
    }

    public class AuthTokensDto
    {
        public string AccessToken { get; set; } = default!;
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@sainin.com",
            password = "TestPassw0rd!"
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResult>(JsonOptions);
        return result!.Data.AccessToken;
    }

    // ---------------------------------------------------------------------------
    // Public — GET /api/v1/secondary-services
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task GetSecondaryServices_Public_Returns200()
    {
        var response = await _client.GetAsync("/api/v1/secondary-services");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.GetProperty("isSuccess").GetBoolean());
    }

    // ---------------------------------------------------------------------------
    // Admin — POST /api/v1/admin/secondary-services
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task PostSecondaryService_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/admin/secondary-services", new
        {
            titleEn = "Test",
            titleAr = "اختبار",
            descriptionEn = "Description",
            descriptionAr = "وصف"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostSecondaryService_WithAuth_Returns201()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/admin/secondary-services", new
        {
            titleEn = $"ServiceEn_{Guid.NewGuid()}",
            titleAr = $"ServiceAr_{Guid.NewGuid()}",
            descriptionEn = "English description",
            descriptionAr = "وصف عربي"
        });

        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.GetProperty("isSuccess").GetBoolean());
    }

    [Fact]
    public async Task PostSecondaryService_WithAutoIncrementDisplayOrder_AppendsToEnd()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create two services without specifying displayOrder — both should succeed
        var r1 = await _client.PostAsJsonAsync("/api/v1/admin/secondary-services", new
        {
            titleEn = $"Auto1En_{Guid.NewGuid()}",
            titleAr = $"Auto1Ar_{Guid.NewGuid()}",
            descriptionEn = "Desc1",
            descriptionAr = "وصف1"
        });
        var r2 = await _client.PostAsJsonAsync("/api/v1/admin/secondary-services", new
        {
            titleEn = $"Auto2En_{Guid.NewGuid()}",
            titleAr = $"Auto2Ar_{Guid.NewGuid()}",
            descriptionEn = "Desc2",
            descriptionAr = "وصف2"
        });

        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.Created, r1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, r2.StatusCode);
    }

    // ---------------------------------------------------------------------------
    // Admin — PUT /api/v1/admin/secondary-services/{id}
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task PutSecondaryService_WithAuth_Returns200()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/admin/secondary-services", new
        {
            titleEn = $"UpdateableEn_{Guid.NewGuid()}",
            titleAr = $"UpdateableAr_{Guid.NewGuid()}",
            descriptionEn = "Desc",
            descriptionAr = "وصف"
        });
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var id = JsonDocument.Parse(createBody).RootElement.GetProperty("data").GetString()!;

        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/admin/secondary-services/{id}", new
        {
            id = id,
            titleEn = $"UpdatedEn_{Guid.NewGuid()}",
            titleAr = $"UpdatedAr_{Guid.NewGuid()}",
            descriptionEn = "Updated description",
            descriptionAr = "وصف محدث",
            displayOrder = 0
        });

        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
    }

    // ---------------------------------------------------------------------------
    // Admin — DELETE /api/v1/admin/secondary-services/{id}
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task DeleteSecondaryService_WithAuth_Returns204()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/admin/secondary-services", new
        {
            titleEn = $"ToDeleteEn_{Guid.NewGuid()}",
            titleAr = $"ToDeleteAr_{Guid.NewGuid()}",
            descriptionEn = "Desc",
            descriptionAr = "وصف"
        });
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var id = JsonDocument.Parse(createBody).RootElement.GetProperty("data").GetString()!;

        var deleteResponse = await _client.DeleteAsync($"/api/v1/admin/secondary-services/{id}");
        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteSecondaryService_NonExistentId_Returns400_WithResourceNotFound()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/admin/secondary-services/{Guid.NewGuid()}");
        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);
        var body = await deleteResponse.Content.ReadAsStringAsync();
        Assert.Contains("RESOURCE_NOT_FOUND", body);
    }
}
