using MediatR;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;

namespace MediaPortfolio.Application.Features.Identity.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<Result<AuthTokensDto>>
{
    public string RefreshToken { get; set; } = string.Empty;
}
