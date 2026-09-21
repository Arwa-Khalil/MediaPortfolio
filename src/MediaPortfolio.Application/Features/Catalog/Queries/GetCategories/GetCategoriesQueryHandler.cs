using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;
using MediaPortfolio.Domain.Enums;

namespace MediaPortfolio.Application.Features.Catalog.Queries.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        // A single query:
        //   FROM Categories c
        //   LEFT JOIN VideoCategories vc ON vc.CategoryId = c.Id
        //   LEFT JOIN Videos v ON vc.VideoId = v.Id AND v.Status = Published AND v.IsDeleted = false
        //   GROUP BY c.Id, c.NameEn, c.NameAr
        //   ORDER BY c.NameEn
        //
        // The EF Global Query Filter on Category (IsDeleted = false) is applied automatically.
        // The Global Query Filter on Video (IsDeleted = false) is applied automatically when
        // Videos is accessed via navigation, but we access VideoCategories directly here, so
        // we guard v.IsDeleted explicitly in the join predicate within the correlated count.

        var categories = await _context.Categories
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
                VideoCount = c.VideoCategories
                    .Where(vc => vc.Video != null
                                 && vc.Video.Status == VideoStatus.Published
                                 && !vc.Video.IsDeleted)
                    .Count()
            })
            .ToListAsync(cancellationToken);

        return Result<List<CategoryDto>>.Success(categories);
    }
}
