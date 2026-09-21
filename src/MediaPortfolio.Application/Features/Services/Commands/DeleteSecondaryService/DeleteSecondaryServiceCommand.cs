using System;
using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Services.Commands.DeleteSecondaryService;

public class DeleteSecondaryServiceCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; set; }
}
