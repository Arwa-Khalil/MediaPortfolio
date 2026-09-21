using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Application.Features.Identity.Commands.LoginAdmin;
using System;

namespace MediaPortfolio.Application.Features.Identity.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthTokensDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IApplicationDbContext dbContext, IJwtService jwtService)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthTokensDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.AdminUsers.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

        if (user == null || user.RefreshTokenExpiresAt < DateTimeOffset.UtcNow)
        {
            return Result<AuthTokensDto>.Failure("TOKEN_INVALID", "Invalid or expired refresh token.");
        }

        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(30);

        return Result<AuthTokensDto>.Success(new AuthTokensDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }
}
