using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Identity.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordCommandHandler(IApplicationDbContext dbContext, IEmailSender emailSender)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
    }

    public async Task<Result<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.AdminUsers.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user != null)
        {
            var resetToken = Guid.NewGuid().ToString("N");
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(1);
            await _emailSender.SendPasswordResetEmailAsync(user.Email, resetToken);
        }

        // Always return success to avoid email enumeration
        return Result<bool>.Success(true);
    }
}
