using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RichardSzalay.MockHttp;
using Xunit;
using MediaPortfolio.Infrastructure.Services;

namespace MediaPortfolio.Infrastructure.UnitTests.Services;

public class TurnstileVerificationServiceTests
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly HttpClient _httpClient;

    public TurnstileVerificationServiceTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = _mockHttp.ToHttpClient();
    }

    [Fact]
    public async Task VerifyAsync_ReturnsTrue_WhenApiReturnsSuccessTrue()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string> {
            { "Turnstile:SecretKey", "1x0000000000000000000000000000000AA" }
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _mockHttp.When(HttpMethod.Post, "https://challenges.cloudflare.com/turnstile/v0/siteverify")
                 .Respond("application/json", "{ \"success\": true }");

        var service = new TurnstileVerificationService(_httpClient, configuration);

        // Act
        var result = await service.VerifyAsync("dummy-token", "127.0.0.1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task VerifyAsync_ReturnsFalse_WhenApiReturnsSuccessFalse()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string> {
            { "Turnstile:SecretKey", "2x0000000000000000000000000000000AA" }
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _mockHttp.When(HttpMethod.Post, "https://challenges.cloudflare.com/turnstile/v0/siteverify")
                 .Respond("application/json", "{ \"success\": false, \"error-codes\": [\"invalid-input-response\"] }");

        var service = new TurnstileVerificationService(_httpClient, configuration);

        // Act
        var result = await service.VerifyAsync("invalid-token", null);

        // Assert
        Assert.False(result);
    }
}
