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

public class CloudflareImageStorageServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<CloudflareImageStorageService>> _loggerMock;
    private readonly MockHttpMessageHandler _mockHttp;

    public CloudflareImageStorageServiceTests()
    {
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<CloudflareImageStorageService>>();
        _mockHttp = new MockHttpMessageHandler();
    }

    [Fact]
    public async Task GetUploadUrlAsync_ReturnsCorrectResult_WhenApiSucceeds()
    {
        // Arrange
        _configMock.Setup(x => x["Cloudflare:ImagesAccountId"]).Returns("test-image-account-id");
        _configMock.Setup(x => x["Cloudflare:ImagesApiToken"]).Returns("test-image-token");

        var responseJson = @"{
            ""result"": {
                ""uploadURL"": ""https://upload.imagedelivery.net/stub-image-url"",
                ""id"": ""stub-image-id""
            },
            ""success"": true
        }";

        _mockHttp.When(HttpMethod.Post, "https://api.cloudflare.com/client/v4/accounts/test-image-account-id/images/v2/direct_upload")
            .WithHeaders("Authorization", "Bearer test-image-token")
            .Respond("application/json", responseJson);

        var httpClient = _mockHttp.ToHttpClient();
        var service = new CloudflareImageStorageService(httpClient, _configMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetUploadUrlAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://upload.imagedelivery.net/stub-image-url", result.UploadUrl);
        Assert.Equal("stub-image-id", result.AssetId);
    }
}
