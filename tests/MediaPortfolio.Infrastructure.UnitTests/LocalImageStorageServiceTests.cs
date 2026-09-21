using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;
using MediaPortfolio.Infrastructure.Services;

namespace MediaPortfolio.Infrastructure.UnitTests;

public class LocalImageStorageServiceTests
{
    private class StubHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }

    [Fact]
    public async Task GetUploadUrlAsync_WithHttpContext_UsesRequestHost()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("test.local:1234");

        var accessor = new StubHttpContextAccessor { HttpContext = context };
        var service = new LocalImageStorageService(accessor);

        var result = await service.GetUploadUrlAsync();

        Assert.NotNull(result);
        Assert.NotNull(result.AssetId);
        Assert.StartsWith($"https://test.local:1234/local-media/upload/image/{result.AssetId}", result.UploadUrl);
    }

    [Fact]
    public async Task GetUploadUrlAsync_WithoutHttpContext_UsesFallbackUrl()
    {
        var accessor = new StubHttpContextAccessor { HttpContext = null };
        var service = new LocalImageStorageService(accessor);

        var result = await service.GetUploadUrlAsync();

        Assert.NotNull(result);
        Assert.NotNull(result.AssetId);
        Assert.StartsWith($"http://localhost:5079/local-media/upload/image/{result.AssetId}", result.UploadUrl);
    }
}
