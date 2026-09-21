using System.Collections.Generic;
using MediatR;
using MediaPortfolio.Application.Common;

namespace MediaPortfolio.Application.Features.Catalog.Queries.GetCategories;

public class GetCategoriesQuery : IRequest<Result<List<CategoryDto>>>
{
}
