using System;
using MediatR;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;

namespace MediaPortfolio.Application.Features.Catalog.Queries.GetPublicVideos;

public class GetPublicVideosQuery : IRequest<Result<PagedResult<PublicVideoDto>>>
{
    public Guid? CategoryId { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
