using System;
using MediatR;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;

namespace MediaPortfolio.Application.Features.Catalog.Queries.GetPublicVideoById;

public class GetPublicVideoByIdQuery : IRequest<Result<PublicVideoDto>>
{
    public Guid Id { get; set; }
}
