using System;
using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Catalog.Commands.DeleteVideo;

public class DeleteVideoCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}
