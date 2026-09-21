using System;
using MediatR;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;

namespace MediaPortfolio.Application.Features.Identity.Commands.LoginAdmin;

public class LoginAdminCommand : IRequest<Result<AuthTokensDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
