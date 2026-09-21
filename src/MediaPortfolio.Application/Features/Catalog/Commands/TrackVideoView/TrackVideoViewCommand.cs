using System;
using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Catalog.Commands.TrackVideoView;

public class TrackVideoViewCommand : IRequest<Result<Unit>>
{
    public Guid VideoId { get; set; }
}
