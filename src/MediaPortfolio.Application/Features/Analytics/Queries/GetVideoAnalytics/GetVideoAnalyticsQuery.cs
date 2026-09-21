using System.Collections.Generic;
using MediatR;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.DTOs;

namespace MediaPortfolio.Application.Features.Analytics.Queries.GetVideoAnalytics;

public class GetVideoAnalyticsQuery : IRequest<Result<IEnumerable<AdminVideoDto>>>
{
    // Flat unpaginated list, defaults to sorting by ViewCount DESC
    // Client can do further filtering/sorting in memory since catalog size is small.
}
