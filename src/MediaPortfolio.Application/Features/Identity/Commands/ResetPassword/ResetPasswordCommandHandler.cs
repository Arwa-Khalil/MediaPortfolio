using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Domain.Entities;

namespace MediaPortfolio.Application.Features.Identity.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher<AdminUser> _passwordHasher;

    public ResetPasswordCommandHandler(IApplicationDbContext dbContext, IPasswordHasher<AdminUser> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.AdminUsers.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token, cancellationToken);

        if (user == null || user.PasswordResetTokenExpiresAt < DateTimeOffset.UtcNow)
        {
            return Result<bool>.Failure("INVALID_TOKEN", "Invalid or expired password reset token.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        user.MustChangePassword = false;

        return Result<bool>.Success(true);
    }
}
