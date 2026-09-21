using MediatR;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;

namespace MediaPortfolio.Application.Features.Catalog.Queries.GetAdminVideos;

public class GetAdminVideosQuery : IRequest<Result<PagedResult<AdminVideoDto>>>
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
