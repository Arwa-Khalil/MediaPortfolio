using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MediaPortfolio.API.IntegrationTests;

/// <summary>
/// Integration tests that require a running PostgreSQL database via Testcontainers.
/// The factory is shared across all tests in this class.
/// </summary>
public class HealthCheckTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthCheckTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOk_WhenDatabaseIsAvailable()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
