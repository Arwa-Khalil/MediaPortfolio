using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Identity.Commands.LogoutAdmin;

public class LogoutAdminCommand : IRequest<Result<bool>>
{
    public string RefreshToken { get; set; } = string.Empty;
}
