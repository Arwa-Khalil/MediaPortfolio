using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Identity.Commands.ChangePassword;

public class ChangePasswordCommand : IRequest<Result<bool>>
{
    public string Email { get; set; } = string.Empty;
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
