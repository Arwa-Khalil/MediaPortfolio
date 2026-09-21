using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MediaPortfolio.API.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for Category endpoints (Admin + Public).
/// Tests run against a real PostgreSQL Testcontainers database.
/// </summary>
public class CategoryEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public CategoryEndpointsTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    public class AuthResult
    {
        public AuthTokensDto Data { get; set; } = default!;
    }

    public class AuthTokensDto
    {
        public string AccessToken { get; set; } = default!;
    }

    private static string? _cachedToken;

    // ---------------------------------------------------------------------------
    // Auth helper — POST /api/v1/auth/login with the seeded admin account
    // ---------------------------------------------------------------------------
    private async Task<string> GetAdminTokenAsync()
    {
        if (_cachedToken != null) return _cachedToken;

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@sainin.com",
            password = "TestPassw0rd!"
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthResult>(JsonOptions);
        _cachedToken = result!.Data.AccessToken;
        return _cachedToken;
    }

    // ---------------------------------------------------------------------------
    // Public — GET /api/v1/categories
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task GetCategories_Public_Returns200_WithEmptyOrPopulatedList()
    {
        var response = await _client.GetAsync("/api/v1/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.GetProperty("isSuccess").GetBoolean());
    }

    // ---------------------------------------------------------------------------
    // Admin — POST /api/v1/admin/categories
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task PostCategory_WithoutAuth_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/admin/categories", new
        {
            nameEn = "Test",
            nameAr = "اختبار"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostCategory_WithAuth_Returns201_AndCreatesCategory()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/admin/categories", new
        {
            nameEn = $"CategoryEn_{Guid.NewGuid()}",
            nameAr = $"CategoryAr_{Guid.NewGuid()}"
        });

        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.GetProperty("isSuccess").GetBoolean());
    }

    [Fact]
    public async Task PostCategory_DuplicateName_Returns400_WithDuplicateCategoryNameError()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var uniqueName = $"DupEn_{Guid.NewGuid()}";
        var uniqueAr = $"DupAr_{Guid.NewGuid()}";

        await _client.PostAsJsonAsync("/api/v1/admin/categories", new { nameEn = uniqueName, nameAr = uniqueAr });
        var secondResponse = await _client.PostAsJsonAsync("/api/v1/admin/categories", new { nameEn = uniqueName, nameAr = uniqueAr });

        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
        var body = await secondResponse.Content.ReadAsStringAsync();
        Assert.Contains("DUPLICATE_CATEGORY_NAME", body);
    }

    // ---------------------------------------------------------------------------
    // Admin — PUT /api/v1/admin/categories/{id}
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task PutCategory_WithAuth_Returns200()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create first
        var createResponse = await _client.PostAsJsonAsync("/api/v1/admin/categories", new
        {
            nameEn = $"UpdateableEn_{Guid.NewGuid()}",
            nameAr = $"UpdateableAr_{Guid.NewGuid()}"
        });
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createBody);
        var id = createDoc.RootElement.GetProperty("data").GetString()!;

        // Update
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/admin/categories/{id}", new
        {
            id = id,
            nameEn = $"UpdatedEn_{Guid.NewGuid()}",
            nameAr = $"UpdatedAr_{Guid.NewGuid()}"
        });

        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
    }

    // ---------------------------------------------------------------------------
    // Admin — DELETE /api/v1/admin/categories/{id}
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task DeleteCategory_WithAuth_Returns204_AndCategoryIsGone()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create
        var createResponse = await _client.PostAsJsonAsync("/api/v1/admin/categories", new
        {
            nameEn = $"ToDeleteEn_{Guid.NewGuid()}",
            nameAr = $"ToDeleteAr_{Guid.NewGuid()}"
        });
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createBody);
        var id = createDoc.RootElement.GetProperty("data").GetString()!;

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/v1/admin/categories/{id}");
        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_NonExistentId_Returns400_WithResourceNotFound()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/admin/categories/{Guid.NewGuid()}");
        _client.DefaultRequestHeaders.Authorization = null;

        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);
        var body = await deleteResponse.Content.ReadAsStringAsync();
        Assert.Contains("RESOURCE_NOT_FOUND", body);
    }

    [Fact]
    public async Task GetCategories_VideoCount_ReturnsOneForCategoryWithOnePublishedVideo()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 1. Create Category
        var catResponse = await _client.PostAsJsonAsync("/api/v1/admin/categories", new
        {
            nameEn = $"CatCountEn_{Guid.NewGuid()}",
            nameAr = $"CatCountAr_{Guid.NewGuid()}"
        });
        var catDoc = JsonDocument.Parse(await catResponse.Content.ReadAsStringAsync());
        var categoryId = catDoc.RootElement.GetProperty("data").GetString()!;

        // 2. Create Published Video in that category
        await _client.PostAsJsonAsync("/api/v1/admin/videos", new
        {
            titleAr = "Count Test Video",
            descriptionAr = "Desc",
            cloudflareStreamId = "stream-123",
            categoryIds = new[] { categoryId },
            status = 1 // Published
        });

        _client.DefaultRequestHeaders.Authorization = null;

        // 3. Fetch public categories
        var response = await _client.GetAsync("/api/v1/categories");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var categories = doc.RootElement.GetProperty("data").EnumerateArray();
        
        bool found = false;
        foreach (var cat in categories)
        {
            if (cat.GetProperty("id").GetString() == categoryId)
            {
                Assert.Equal(1, cat.GetProperty("videoCount").GetInt32());
                found = true;
                break;
            }
        }
        
        Assert.True(found, "The created category was not found in the public list.");
    }
}
