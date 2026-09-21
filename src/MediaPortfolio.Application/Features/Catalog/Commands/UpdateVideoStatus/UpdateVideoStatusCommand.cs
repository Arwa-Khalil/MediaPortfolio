using System;
using MediatR;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Application.Features.Catalog.Commands.UpdateVideoStatus;

public class UpdateVideoStatusCommand : IRequest<Result<Guid>>
{
    public Guid Id { get; set; }
    public VideoStatus Status { get; set; }
}
