using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MediaPortfolio.API.Models;
using MediaPortfolio.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MediaPortfolio.API.IntegrationTests.Endpoints;

public class AuthEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointsTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TokenIsolation_ForgotPassword_ShouldNotInvalidateRefreshToken()
    {
        // 1. Log in to get a refresh token
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@sainin.com",
            password = "TestPassw0rd!" // Based on Seed Data
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<ApiResponse<AuthTokensDto>>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        var refreshToken = loginResult!.Data!.RefreshToken;
        Assert.False(string.IsNullOrEmpty(refreshToken));

        // 2. Trigger forgot-password
        var forgotPasswordResponse = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new
        {
            email = "admin@sainin.com"
        });

        Assert.Equal(HttpStatusCode.OK, forgotPasswordResponse.StatusCode);

        // 3. Assert refresh token is unchanged and still valid by performing a refresh
        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken = refreshToken
        });

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        // 4. Assert submitting the refresh token to reset-password is rejected
        var resetPasswordResponse = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            token = refreshToken, // Attempt to use refresh token as reset token
            newPassword = "NewPassword123!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, resetPasswordResponse.StatusCode);
        var resetContent = await resetPasswordResponse.Content.ReadAsStringAsync();
        var resetResult = JsonSerializer.Deserialize<ApiResponse<object>>(resetContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        Assert.False(resetResult!.IsSuccess);
        Assert.Equal("INVALID_TOKEN", resetResult.Errors![0].Code);
        
        // Let's also query the DB directly to prove independence.
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MediaPortfolio.Application.Interfaces.IApplicationDbContext>();
        var user = await dbContext.AdminUsers.FirstOrDefaultAsync(u => u.Email == "admin@sainin.com");
        
        Assert.NotNull(user);
        Assert.False(string.IsNullOrEmpty(user!.RefreshToken));
        Assert.False(string.IsNullOrEmpty(user.PasswordResetToken));
        Assert.NotEqual(user.PasswordResetToken, user.RefreshToken);
    }
}
