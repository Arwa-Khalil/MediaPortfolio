using MediatR;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;

namespace MediaPortfolio.Application.Features.Catalog.Queries.SearchPublicVideos;

public class SearchPublicVideosQuery : IRequest<Result<PagedResult<PublicVideoDto>>>
{
    public string? Q { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}
