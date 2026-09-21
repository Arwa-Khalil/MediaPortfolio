using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Domain.Entities;
using System;

namespace MediaPortfolio.Application.Features.Identity.Commands.LoginAdmin;

public interface IJwtService
{
    string GenerateAccessToken(AdminUser user);
    string GenerateRefreshToken();
}

public class LoginAdminCommandHandler : IRequestHandler<LoginAdminCommand, Result<AuthTokensDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher<AdminUser> _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginAdminCommandHandler(
        IApplicationDbContext dbContext,
        IPasswordHasher<AdminUser> passwordHasher,
        IJwtService jwtService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<Result<AuthTokensDto>> Handle(LoginAdminCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.AdminUsers.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            return Result<AuthTokensDto>.Failure("INVALID_CREDENTIALS", "Invalid email or password.");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result != PasswordVerificationResult.Success)
        {
            return Result<AuthTokensDto>.Failure("INVALID_CREDENTIALS", "Invalid email or password.");
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
        
        return Result<AuthTokensDto>.Success(new AuthTokensDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }
}
