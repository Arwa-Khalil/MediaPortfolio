using System.Collections.Generic;
using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Services.Queries.GetSecondaryServices;

public class GetSecondaryServicesQuery : IRequest<Result<List<SecondaryServiceDto>>>
{
}
