using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Identity.Commands.ResetPassword;

public class ResetPasswordCommand : IRequest<Result<bool>>
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
