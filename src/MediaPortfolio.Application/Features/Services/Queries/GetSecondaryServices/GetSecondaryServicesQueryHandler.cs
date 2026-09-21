using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediaPortfolio.Application.Common;
using MediaPortfolio.Application.Interfaces;

namespace MediaPortfolio.Application.Features.Services.Queries.GetSecondaryServices;

public class GetSecondaryServicesQueryHandler : IRequestHandler<GetSecondaryServicesQuery, Result<List<SecondaryServiceDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetSecondaryServicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<SecondaryServiceDto>>> Handle(GetSecondaryServicesQuery request, CancellationToken cancellationToken)
    {
        var services = await _context.SecondaryServices
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new SecondaryServiceDto
            {
                Id = s.Id,
                TitleEn = s.TitleEn,
                TitleAr = s.TitleAr,
                DescriptionEn = s.DescriptionEn,
                DescriptionAr = s.DescriptionAr,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return Result<List<SecondaryServiceDto>>.Success(services);
    }
}
