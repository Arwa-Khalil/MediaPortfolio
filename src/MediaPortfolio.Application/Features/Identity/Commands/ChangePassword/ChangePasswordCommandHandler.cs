using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Domain.Entities;

namespace MediaPortfolio.Application.Features.Identity.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher<AdminUser> _passwordHasher;

    public ChangePasswordCommandHandler(IApplicationDbContext dbContext, IPasswordHasher<AdminUser> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.AdminUsers.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            return Result<bool>.Failure("USER_NOT_FOUND", "User not found.");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);

        if (result != PasswordVerificationResult.Success)
        {
            return Result<bool>.Failure("INVALID_PASSWORD", "Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.MustChangePassword = false;

        return Result<bool>.Success(true);
    }
}
