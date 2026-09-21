using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;
using Xunit;
using MediaPortfolio.Infrastructure.Services;

namespace MediaPortfolio.Infrastructure.UnitTests;

public class CloudflareVideoStorageServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<CloudflareVideoStorageService>> _loggerMock;
    private readonly MockHttpMessageHandler _mockHttp;

    public CloudflareVideoStorageServiceTests()
    {
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<CloudflareVideoStorageService>>();
        _mockHttp = new MockHttpMessageHandler();
    }

    [Fact]
    public async Task GetUploadUrlAsync_ReturnsCorrectResult_WhenApiSucceeds()
    {
        // Arrange
        _configMock.Setup(x => x["Cloudflare:StreamAccountId"]).Returns("test-account-id");
        _configMock.Setup(x => x["Cloudflare:StreamApiToken"]).Returns("test-token");

        var responseJson = @"{
            ""result"": {
                ""uploadURL"": ""https://upload.videodelivery.net/stub-url"",
                ""uid"": ""stub-uid""
            },
            ""success"": true
        }";

        _mockHttp.When(HttpMethod.Post, "https://api.cloudflare.com/client/v4/accounts/test-account-id/stream/direct_upload")
            .WithHeaders("Authorization", "Bearer test-token")
            .Respond("application/json", responseJson);

        var httpClient = _mockHttp.ToHttpClient();
        var service = new CloudflareVideoStorageService(httpClient, _configMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetUploadUrlAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://upload.videodelivery.net/stub-url", result.UploadUrl);
        Assert.Equal("stub-uid", result.AssetId);
    }
}
