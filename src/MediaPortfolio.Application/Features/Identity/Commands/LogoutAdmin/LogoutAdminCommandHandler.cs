using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Identity.Commands.LogoutAdmin;

public class LogoutAdminCommandHandler : IRequestHandler<LogoutAdminCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public LogoutAdminCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(LogoutAdminCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.AdminUsers.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
        }

        return Result<bool>.Success(true);
    }
}
