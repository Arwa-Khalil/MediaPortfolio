using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Moq;
using System.Threading;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.Application.Features.Contact.Commands.CreateContactSubmission;
using MediaPortfolio.API.Models;
using System.Collections.Generic;

namespace MediaPortfolio.API.IntegrationTests.Endpoints;

[Collection("SharedDbCollection")]
public class ContactEndpointsTests
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly System.Net.Http.HttpClient _client;

    public ContactEndpointsTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        
        // Reset mock before each test
        _factory.TurnstileMock.Reset();
    }

    [Fact]
    public async Task CreateContactSubmission_ReturnsCreated_WhenValid()
    {
        // Arrange
        _factory.TurnstileMock
            .Setup(x => x.VerifyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateContactSubmissionCommand
        {
            Name = "Jane Doe",
            Phone = "+1234567890",
            Email = "jane@example.com",
            Message = "Test Message",
            TurnstileToken = "valid-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/contact-submissions", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateContactSubmission_ReturnsBadRequest_WhenTurnstileFails()
    {
        // Arrange
        _factory.TurnstileMock
            .Setup(x => x.VerifyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreateContactSubmissionCommand
        {
            Name = "Jane Doe",
            Phone = "+1234567890",
            Email = "jane@example.com",
            Message = "Test Message",
            TurnstileToken = "invalid-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/contact-submissions", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(apiResponse);
        Assert.False(apiResponse.IsSuccess);
        Assert.NotNull(apiResponse.Errors);
        Assert.Contains(apiResponse.Errors, e => e.Field == "turnstileToken");
    }
}
