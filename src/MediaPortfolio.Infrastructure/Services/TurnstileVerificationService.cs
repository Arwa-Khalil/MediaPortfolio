using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Infrastructure.Services;

public class TurnstileVerificationService : ITurnstileVerificationService
{
    private readonly HttpClient _httpClient;
    private readonly string? _secretKey;

    public TurnstileVerificationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _secretKey = configuration["Turnstile:SecretKey"];
    }

    public async Task<bool> VerifyAsync(string token, string? remoteIp, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_secretKey))
        {
            throw new InvalidOperationException("Turnstile SecretKey is not configured.");
        }

        var contentData = new Dictionary<string, string>
        {
            { "secret", _secretKey },
            { "response", token }
        };

        if (!string.IsNullOrWhiteSpace(remoteIp))
        {
            contentData.Add("remoteip", remoteIp);
        }

        var content = new FormUrlEncodedContent(contentData);

        var response = await _httpClient.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", content, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        
        try 
        {
            using var document = JsonDocument.Parse(responseString);
            if (document.RootElement.TryGetProperty("success", out var successProperty))
            {
                return successProperty.GetBoolean();
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }
}
