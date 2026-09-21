using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using MediaPortfolio.Infrastructure.Persistence;

namespace MediaPortfolio.API.IntegrationTests.Endpoints;

public class VideoPublicEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ApiWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    public VideoPublicEndpointsTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static string? _cachedToken;

    private async Task<string> GetAdminTokenAsync()
    {
        if (_cachedToken != null) return _cachedToken;

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = "admin@sainin.com", password = "TestPassw0rd!" });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        _cachedToken = doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
        return _cachedToken;
    }

    private async Task<string> CreateCategoryAsync(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var res = await _client.PostAsJsonAsync("/api/v1/admin/categories", new { nameEn = $"Cat_{Guid.NewGuid()}", nameAr = $"Cat_{Guid.NewGuid()}" });
        _client.DefaultRequestHeaders.Authorization = null;
        var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("data").GetString()!;
    }

    private async Task<string> CreateVideoAsync(string token, string titleAr, string descAr, int status, string categoryId)
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var res = await _client.PostAsJsonAsync("/api/v1/admin/videos", new
        {
            titleAr = titleAr,
            descriptionAr = descAr,
            cloudflareStreamId = $"stream-{Guid.NewGuid()}",
            categoryIds = new[] { categoryId },
            status = status
        });
        _client.DefaultRequestHeaders.Authorization = null;
        var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("data").GetString()!;
    }

    [Fact]
    public async Task GetVideos_PublicFields_DoNotIncludeTitleArOrDescriptionAr()
    {
        var token = await GetAdminTokenAsync();
        var catId = await CreateCategoryAsync(token);
        await CreateVideoAsync(token, "SecretTitle", "SecretDesc", 1, catId);

        var response = await _client.GetAsync("/api/v1/videos");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("data").EnumerateArray();
        
        Assert.True(items.Any(), "Expected at least one video to test keys.");
        var video = items.First();
        
        Assert.False(video.TryGetProperty("titleAr", out _), "titleAr should not be serialized.");
        Assert.False(video.TryGetProperty("TitleAr", out _), "TitleAr should not be serialized.");
        Assert.False(video.TryGetProperty("descriptionAr", out _), "descriptionAr should not be serialized.");
    }

    [Fact]
    public async Task GetVideos_CategoryFilter_ReturnsOnlyMatchingPublishedVideos()
    {
        var token = await GetAdminTokenAsync();
        var cat1 = await CreateCategoryAsync(token);
        var cat2 = await CreateCategoryAsync(token);
        
        var v1Id = await CreateVideoAsync(token, "V1", "D1", 1, cat1); // Published, cat1
        var v2Id = await CreateVideoAsync(token, "V2", "D2", 1, cat2); // Published, cat2
        var v3Id = await CreateVideoAsync(token, "V3", "D3", 0, cat1); // Draft, cat1

        var response = await _client.GetAsync($"/api/v1/videos?categoryId={cat1}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("data").EnumerateArray().ToList();
        
        // Assert V1 is present, V2 is not (wrong cat), V3 is not (draft)
        Assert.Contains(items, i => i.GetProperty("id").GetString() == v1Id);
        Assert.DoesNotContain(items, i => i.GetProperty("id").GetString() == v2Id);
        Assert.DoesNotContain(items, i => i.GetProperty("id").GetString() == v3Id);
    }

    [Fact]
    public async Task SearchVideos_MatchesTitleAr()
    {
        var token = await GetAdminTokenAsync();
        var catId = await CreateCategoryAsync(token);
        var uniqueMarker = $"TitleMark_{Guid.NewGuid()}";
        var vId = await CreateVideoAsync(token, $"Prefix {uniqueMarker} Suffix", "Desc", 1, catId);

        var response = await _client.GetAsync($"/api/v1/videos/search?q={uniqueMarker}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        var items = doc.RootElement.GetProperty("data").EnumerateArray().ToList();
        
        Assert.Contains(items, i => i.GetProperty("id").GetString() == vId);
    }

    [Fact]
    public async Task SearchVideos_MatchesDescriptionAr()
    {
        var token = await GetAdminTokenAsync();
        var catId = await CreateCategoryAsync(token);
        var uniqueMarker = $"DescMark_{Guid.NewGuid()}";
        var vId = await CreateVideoAsync(token, "Title", $"Prefix {uniqueMarker} Suffix", 1, catId);

        var response = await _client.GetAsync($"/api/v1/videos/search?q={uniqueMarker}");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("data").EnumerateArray().ToList();
        
        Assert.Contains(items, i => i.GetProperty("id").GetString() == vId);
    }

    [Fact]
    public async Task SearchVideos_NoMatch_ReturnsEmptyList()
    {
        var response = await _client.GetAsync($"/api/v1/videos/search?q=NonExistent_{Guid.NewGuid()}");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("data").EnumerateArray().ToList();
        
        Assert.Empty(items);
    }

    [Fact]
    public async Task SearchVideos_CaseInsensitive()
    {
        var token = await GetAdminTokenAsync();
        var catId = await CreateCategoryAsync(token);
        var uniqueMarker = $"CaSeTeSt_{Guid.NewGuid().ToString().Substring(0, 8)}"; // mixed case
        var vId = await CreateVideoAsync(token, $"Title {uniqueMarker}", "Desc", 1, catId);

        var upperSearch = uniqueMarker.ToUpperInvariant();
        var response = await _client.GetAsync($"/api/v1/videos/search?q={upperSearch}");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("data").EnumerateArray().ToList();
        
        Assert.Contains(items, i => i.GetProperty("id").GetString() == vId);
    }

    [Fact]
    public async Task SearchVideos_PublicFields_DoNotIncludeTitleArOrDescriptionAr()
    {
        var token = await GetAdminTokenAsync();
        var catId = await CreateCategoryAsync(token);
        var uniqueMarker = $"SearchKeyTest_{Guid.NewGuid()}";
        await CreateVideoAsync(token, uniqueMarker, "Desc", 1, catId);

        var response = await _client.GetAsync($"/api/v1/videos/search?q={uniqueMarker}");
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("data").EnumerateArray();
        
        Assert.True(items.Any());
        var video = items.First();
        
        Assert.False(video.TryGetProperty("titleAr", out _));
        Assert.False(video.TryGetProperty("descriptionAr", out _));
    }

    [Fact]
    public async Task GetVideoById_PublishedVideo_Returns200()
    {
        var token = await GetAdminTokenAsync();
        var catId = await CreateCategoryAsync(token);
        var vId = await CreateVideoAsync(token, "T", "D", 1, catId);

        var response = await _client.GetAsync($"/api/v1/videos/{vId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        Assert.Equal(vId, doc.RootElement.GetProperty("data").GetProperty("id").GetString());
    }

    [Fact]
    public async Task GetVideoById_DraftVideo_ReturnsIdentical404AsTotallyNonexistentGuid()
    {
        var token = await GetAdminTokenAsync();
        var catId = await CreateCategoryAsync(token);
        var draftId = await CreateVideoAsync(token, "T", "D", 0, catId);
        var randomId = Guid.NewGuid().ToString();

        var draftRes = await _client.GetAsync($"/api/v1/videos/{draftId}");
        var randRes = await _client.GetAsync($"/api/v1/videos/{randomId}");

        Assert.Equal(HttpStatusCode.NotFound, draftRes.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, randRes.StatusCode);

        // Remove timestamp before comparison
        var draftBody = await draftRes.Content.ReadAsStringAsync();
        var randBody = await randRes.Content.ReadAsStringAsync();
        
        using var draftDoc = JsonDocument.Parse(draftBody);
        using var randDoc = JsonDocument.Parse(randBody);
        
        var draftCode = draftDoc.RootElement.GetProperty("errors")[0].GetProperty("code").GetString();
        var randCode = randDoc.RootElement.GetProperty("errors")[0].GetProperty("code").GetString();
        
        Assert.Equal("RESOURCE_NOT_FOUND", draftCode);
        Assert.Equal("RESOURCE_NOT_FOUND", randCode);
    }

    [Fact]
    public async Task GetVideoById_DeletedVideo_Returns404()
    {
        var token = await GetAdminTokenAsync();
        var catId = await CreateCategoryAsync(token);
        var vId = await CreateVideoAsync(token, "T", "D", 1, catId);

        // Delete it
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        await _client.DeleteAsync($"/api/v1/admin/videos/{vId}");
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync($"/api/v1/videos/{vId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TrackView_IncrementsPersisted_ConfirmedViaAdminEndpoint()
    {
        var token = await GetAdminTokenAsync();
        var catId = await CreateCategoryAsync(token);
        var vId = await CreateVideoAsync(token, "T", "D", 1, catId);

        var trackRes = await _client.PostAsync($"/api/v1/videos/{vId}/track-view", null);
        Assert.Equal(HttpStatusCode.OK, trackRes.StatusCode);
        
        var doc = JsonDocument.Parse(await trackRes.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("data").ValueKind);

        // Verify via admin endpoint
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var adminRes = await _client.GetAsync("/api/v1/admin/videos");
        var adminBody = await adminRes.Content.ReadAsStringAsync();
        adminRes.EnsureSuccessStatusCode();
        
        var adminDoc = JsonDocument.Parse(adminBody);
        var video = adminDoc.RootElement.GetProperty("data").EnumerateArray().First(i => i.GetProperty("id").GetString() == vId);
        
        Assert.Equal(1, video.GetProperty("viewCount").GetInt32());
    }

    [Fact]
    public async Task TrackWhatsAppClick_IncrementsPersisted_ConfirmedViaAdminEndpoint()
    {
        var token = await GetAdminTokenAsync();
        var catId = await CreateCategoryAsync(token);
        var vId = await CreateVideoAsync(token, "T", "D", 1, catId);

        var trackRes = await _client.PostAsync($"/api/v1/videos/{vId}/track-whatsapp-click", null);
        Assert.Equal(HttpStatusCode.OK, trackRes.StatusCode);

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var adminRes = await _client.GetAsync("/api/v1/admin/videos");
        var adminBody = await adminRes.Content.ReadAsStringAsync();
        adminRes.EnsureSuccessStatusCode();
        
        var adminDoc = JsonDocument.Parse(adminBody);
        var video = adminDoc.RootElement.GetProperty("data").EnumerateArray().First(i => i.GetProperty("id").GetString() == vId);
        
        Assert.Equal(1, video.GetProperty("whatsAppClickCount").GetInt32());
    }

    [Fact]
    public async Task TrackView_OnDraftVideo_Returns404()
    {
        var token = await GetAdminTokenAsync();
        var catId = await CreateCategoryAsync(token);
        var vId = await CreateVideoAsync(token, "T", "D", 0, catId); // Draft

        var trackRes = await _client.PostAsync($"/api/v1/videos/{vId}/track-view", null);
        Assert.Equal(HttpStatusCode.NotFound, trackRes.StatusCode);
        
        var doc = JsonDocument.Parse(await trackRes.Content.ReadAsStringAsync());
        Assert.Equal("RESOURCE_NOT_FOUND", doc.RootElement.GetProperty("errors")[0].GetProperty("code").GetString());
    }

    [Fact]
    public async Task TrackView_OnNonexistentVideo_Returns404()
    {
        var trackRes = await _client.PostAsync($"/api/v1/videos/{Guid.NewGuid()}/track-view", null);
        Assert.Equal(HttpStatusCode.NotFound, trackRes.StatusCode);
    }

    /// <summary>
    /// API-Contract-v1.2 §4.3: GET /videos returns results ordered by publishedAt DESC, id ASC.
    /// Videos are created in one order but published in a deliberately scrambled order to prove
    /// the sort is genuinely keyed on publishedAt — not on CreatedAt or insertion order.
    /// </summary>
    [Fact]
    public async Task GetVideos_OrderedByPublishedAtDescending_NotByCreationOrder()
    {
        var token = await GetAdminTokenAsync();

        // Create an isolated category for this test so other test data doesn't bleed in.
        var catId = await CreateCategoryAsync(token);

        // Seed directly so we can control PublishedAt independently of CreatedAt.
        // Video A: created first, but given an OLD publishedAt  → should sort last.
        // Video B: created second, given a NEWER publishedAt   → should sort first.
        // Video C: created third, given a MID publishedAt      → should sort second.
        // If the implementation were still sorting by CreatedAt the order would be A, B, C.
        // The correct publishedAt-DESC order is B, C, A.
        var now = DateTimeOffset.UtcNow;
        var publishedAtA = now.AddDays(-10);   // oldest publish
        var publishedAtB = now.AddDays(-1);    // newest publish
        var publishedAtC = now.AddDays(-5);    // mid publish

        Guid idA, idB, idC;
        using (var scope = _factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<MediaPortfolio.Application.Interfaces.IApplicationDbContext>();

            var catGuid = Guid.Parse(catId);

            var videoA = new MediaPortfolio.Domain.Entities.Video
            {
                TitleAr = $"SortTest_A_{Guid.NewGuid()}",
                DescriptionAr = "sort-test",
                CloudflareStreamId = $"stream-sort-a-{Guid.NewGuid()}",
                Status = MediaPortfolio.Domain.Enums.VideoStatus.Published,
                CreatedAt = now.AddDays(-30),    // created long ago
                PublishedAt = publishedAtA        // but published oldest
            };
            videoA.VideoCategories.Add(new MediaPortfolio.Domain.Entities.VideoCategory { VideoId = videoA.Id, CategoryId = catGuid });

            var videoB = new MediaPortfolio.Domain.Entities.Video
            {
                TitleAr = $"SortTest_B_{Guid.NewGuid()}",
                DescriptionAr = "sort-test",
                CloudflareStreamId = $"stream-sort-b-{Guid.NewGuid()}",
                Status = MediaPortfolio.Domain.Enums.VideoStatus.Published,
                CreatedAt = now.AddDays(-20),    // created later than A
                PublishedAt = publishedAtB        // but published most recently
            };
            videoB.VideoCategories.Add(new MediaPortfolio.Domain.Entities.VideoCategory { VideoId = videoB.Id, CategoryId = catGuid });

            var videoC = new MediaPortfolio.Domain.Entities.Video
            {
                TitleAr = $"SortTest_C_{Guid.NewGuid()}",
                DescriptionAr = "sort-test",
                CloudflareStreamId = $"stream-sort-c-{Guid.NewGuid()}",
                Status = MediaPortfolio.Domain.Enums.VideoStatus.Published,
                CreatedAt = now.AddDays(-10),    // created most recently
                PublishedAt = publishedAtC        // but published in the middle
            };
            videoC.VideoCategories.Add(new MediaPortfolio.Domain.Entities.VideoCategory { VideoId = videoC.Id, CategoryId = catGuid });

            ctx.Videos.Add(videoA);
            ctx.Videos.Add(videoB);
            ctx.Videos.Add(videoC);
            await ctx.SaveChangesAsync(default);

            idA = videoA.Id;
            idB = videoB.Id;
            idC = videoC.Id;
        }

        // Call the public endpoint — two identical calls to prove stability.
        for (int pass = 1; pass <= 2; pass++)
        {
            var response = await _client.GetAsync($"/api/v1/videos?categoryId={catId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var ids = doc.RootElement.GetProperty("data").EnumerateArray()
                .Select(v => Guid.Parse(v.GetProperty("id").GetString()!))
                .ToList();

            // Extract only our three seeded videos (there may be others in the DB from other tests)
            var ours = ids.Where(id => id == idA || id == idB || id == idC).ToList();
            Assert.Equal(3, ours.Count);

            // Expected: B (newest publishedAt) → C (mid) → A (oldest) — publishedAt DESC
            Assert.Equal(idB, ours[0]);
            Assert.Equal(idC, ours[1]);
            Assert.Equal(idA, ours[2]);
        }
    }

    /// <summary>
    /// API-Contract-v1.2 §4.3: When two Published videos share the exact same publishedAt,
    /// they must be deterministically ordered by id ascending as the tiebreaker.
    /// </summary>
    [Fact]
    public async Task GetVideos_TiebreakerById_WhenPublishedAtIsIdentical()
    {
        var catId = await CreateCategoryAsync(await GetAdminTokenAsync());
        var tiedTimestamp = DateTimeOffset.UtcNow.AddDays(-3);
        Guid idX, idY;

        using (var scope = _factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<MediaPortfolio.Application.Interfaces.IApplicationDbContext>();
            var catGuid = Guid.Parse(catId);

            // Force deterministic Guid order: create both, then swap the Ids so we know which is smaller.
            var rawX = Guid.NewGuid();
            var rawY = Guid.NewGuid();
            // Ensure X < Y (Guid comparison) so the assertion is predictable regardless of random values.
            var smallerId = rawX.CompareTo(rawY) < 0 ? rawX : rawY;
            var largerId  = rawX.CompareTo(rawY) < 0 ? rawY : rawX;

            var videoSmall = new MediaPortfolio.Domain.Entities.Video
            {
                Id = smallerId,
                TitleAr = $"TieTest_Small_{Guid.NewGuid()}",
                DescriptionAr = "tiebreaker-test",
                CloudflareStreamId = $"stream-tie-small-{Guid.NewGuid()}",
                Status = MediaPortfolio.Domain.Enums.VideoStatus.Published,
                PublishedAt = tiedTimestamp
            };
            videoSmall.VideoCategories.Add(new MediaPortfolio.Domain.Entities.VideoCategory { VideoId = smallerId, CategoryId = catGuid });

            var videoLarge = new MediaPortfolio.Domain.Entities.Video
            {
                Id = largerId,
                TitleAr = $"TieTest_Large_{Guid.NewGuid()}",
                DescriptionAr = "tiebreaker-test",
                CloudflareStreamId = $"stream-tie-large-{Guid.NewGuid()}",
                Status = MediaPortfolio.Domain.Enums.VideoStatus.Published,
                PublishedAt = tiedTimestamp
            };
            videoLarge.VideoCategories.Add(new MediaPortfolio.Domain.Entities.VideoCategory { VideoId = largerId, CategoryId = catGuid });

            ctx.Videos.Add(videoSmall);
            ctx.Videos.Add(videoLarge);
            await ctx.SaveChangesAsync(default);

            idX = smallerId;
            idY = largerId;
        }

        // Call twice to confirm stability.
        for (int pass = 1; pass <= 2; pass++)
        {
            var response = await _client.GetAsync($"/api/v1/videos?categoryId={catId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var ids = doc.RootElement.GetProperty("data").EnumerateArray()
                .Select(v => Guid.Parse(v.GetProperty("id").GetString()!))
                .Where(id => id == idX || id == idY)
                .ToList();

            Assert.Equal(2, ids.Count);
            // id ASC tiebreaker: smaller Guid must come first.
            Assert.Equal(idX, ids[0]); // X is the smaller Guid
            Assert.Equal(idY, ids[1]);
        }
    }

    /// <summary>
    /// API-Contract-v1.2 §4.3: GET /videos/search must apply the same publishedAt DESC, id ASC
    /// ordering guarantee as GET /videos — the same sort contract, independently confirmed.
    /// </summary>
    [Fact]
    public async Task SearchVideos_OrderedByPublishedAtDescending_MatchesGetVideosContract()
    {
        var token = await GetAdminTokenAsync();
        var catId = await CreateCategoryAsync(token);
        var uniqueTag = $"SearchSort_{Guid.NewGuid()}";
        var now = DateTimeOffset.UtcNow;

        Guid idP, idQ, idR;
        using (var scope = _factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<MediaPortfolio.Application.Interfaces.IApplicationDbContext>();
            var catGuid = Guid.Parse(catId);

            // P: created first, published last → should sort last (publishedAt oldest)
            // Q: created second, published first → should sort first (publishedAt newest)
            // R: created third, published middle → should sort second
            var videoP = new MediaPortfolio.Domain.Entities.Video
            {
                TitleAr = $"{uniqueTag}_P",
                DescriptionAr = $"SearchSort test video P",
                CloudflareStreamId = $"stream-ss-p-{Guid.NewGuid()}",
                Status = MediaPortfolio.Domain.Enums.VideoStatus.Published,
                CreatedAt = now.AddDays(-15),
                PublishedAt = now.AddDays(-12)   // oldest publish
            };
            videoP.VideoCategories.Add(new MediaPortfolio.Domain.Entities.VideoCategory { VideoId = videoP.Id, CategoryId = catGuid });

            var videoQ = new MediaPortfolio.Domain.Entities.Video
            {
                TitleAr = $"{uniqueTag}_Q",
                DescriptionAr = $"SearchSort test video Q",
                CloudflareStreamId = $"stream-ss-q-{Guid.NewGuid()}",
                Status = MediaPortfolio.Domain.Enums.VideoStatus.Published,
                CreatedAt = now.AddDays(-10),
                PublishedAt = now.AddDays(-2)    // newest publish
            };
            videoQ.VideoCategories.Add(new MediaPortfolio.Domain.Entities.VideoCategory { VideoId = videoQ.Id, CategoryId = catGuid });

            var videoR = new MediaPortfolio.Domain.Entities.Video
            {
                TitleAr = $"{uniqueTag}_R",
                DescriptionAr = $"SearchSort test video R",
                CloudflareStreamId = $"stream-ss-r-{Guid.NewGuid()}",
                Status = MediaPortfolio.Domain.Enums.VideoStatus.Published,
                CreatedAt = now.AddDays(-5),
                PublishedAt = now.AddDays(-7)    // middle publish
            };
            videoR.VideoCategories.Add(new MediaPortfolio.Domain.Entities.VideoCategory { VideoId = videoR.Id, CategoryId = catGuid });

            ctx.Videos.Add(videoP);
            ctx.Videos.Add(videoQ);
            ctx.Videos.Add(videoR);
            await ctx.SaveChangesAsync(default);

            idP = videoP.Id;
            idQ = videoQ.Id;
            idR = videoR.Id;
        }

        // Two identical calls to prove stability.
        for (int pass = 1; pass <= 2; pass++)
        {
            var response = await _client.GetAsync($"/api/v1/videos/search?q={uniqueTag}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var ids = doc.RootElement.GetProperty("data").EnumerateArray()
                .Select(v => Guid.Parse(v.GetProperty("id").GetString()!))
                .Where(id => id == idP || id == idQ || id == idR)
                .ToList();

            Assert.Equal(3, ids.Count);
            // Expected: Q (newest publishedAt) → R (mid) → P (oldest) — publishedAt DESC
            Assert.Equal(idQ, ids[0]);
            Assert.Equal(idR, ids[1]);
            Assert.Equal(idP, ids[2]);
        }
    }
}

/// <summary>
/// Dedicated factory for the rate limit test to ensure it has a clean state 
/// and doesn't interfere with other tests hitting tracking endpoints.
/// </summary>
public class IsolatedRateLimitWebApplicationFactory : ApiWebApplicationFactory { }

public class RateLimitEndpointTests : IClassFixture<IsolatedRateLimitWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RateLimitEndpointTests(IsolatedRateLimitWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RateLimit_TrackingEndpoints_Returns429AfterExceedingLimit()
    {
        // Limit is 30 per 10 seconds. We loop 31 times.
        var videoId = Guid.NewGuid().ToString(); // doesn't matter if it exists, rate limit runs first

        HttpResponseMessage lastResponse = null!;
        for (int i = 0; i < 35; i++) // safety margin
        {
            lastResponse = await _client.PostAsync($"/api/v1/videos/{videoId}/track-view", null);
            if (lastResponse.StatusCode == HttpStatusCode.TooManyRequests)
                break;
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, lastResponse.StatusCode);
        
        var body = await lastResponse.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(body);
        
        Assert.False(doc.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.Equal("RATE_LIMITED", doc.RootElement.GetProperty("errors")[0].GetProperty("code").GetString());
    }
}
