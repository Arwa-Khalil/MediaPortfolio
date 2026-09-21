using System;
using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Catalog.Commands.TrackWhatsAppClick;

public class TrackWhatsAppClickCommand : IRequest<Result<Unit>>
{
    public Guid VideoId { get; set; }
}
