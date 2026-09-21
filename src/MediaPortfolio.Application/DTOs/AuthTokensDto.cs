using System;

namespace MediaPortfolio.Application.DTOs;

public class AuthTokensDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
