using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Identity.Commands.ForgotPassword;

public class ForgotPasswordCommand : IRequest<Result<bool>>
{
    public string Email { get; set; } = string.Empty;
}
